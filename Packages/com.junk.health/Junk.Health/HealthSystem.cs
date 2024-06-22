using Junk.Entities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Hitpoints
{
    public struct DamageWorldSingleton : IComponentData
    {
        public DamageWorld DamageWorld;

        public NativeStream.Writer GetDamageStreamWriter()
        {
            return DamageWorld.GetDamageWriter();
        }
        public NativeStream.Reader GetDamageStreamReader()
        {
            return DamageWorld.GetDamageReader();
        }
        
        public int Count => DamageWorld.DamageEventDataStream.Count();
        public void Dispose()
        {
            DamageWorld.Dispose();
        }
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        private EntityQuery healthQuery;
        private EntityQuery updatedDamageQuery;
        private EntityQuery damageUpdateWithHealthParentQuery;
        private EntityQuery collectDamageQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // New damage event query
            collectDamageQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DamageData>()
                .Build(ref state);
            
            // Query for damage buffer changes to apply to linked healths, via HealthParent
            damageUpdateWithHealthParentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<HealthDamageBuffer>()
                .WithAll<HealthParent>()
                .Build(ref state);
            damageUpdateWithHealthParentQuery.SetChangedVersionFilter(ComponentType.ReadWrite<HealthDamageBuffer>());
            
            // Query for damage buffer changes to apply to Health
            updatedDamageQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<HealthDamageBuffer>()
                .WithAll<HealthData>()
                .Build(ref state);
            updatedDamageQuery.SetChangedVersionFilter(ComponentType.ReadWrite<HealthDamageBuffer>());
            
            // Create the damage world singleton
            state.EntityManager.AddComponentData(state.SystemHandle, new DamageWorldSingleton()
            {
                DamageWorld = new DamageWorld(100, state.EntityManager)
            });
        }
        
        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        private struct GatherDamageInstancesJob : IJobChunk
        {
            public            EntityCommandBuffer                 DestroyCommandBuffer;
            public            EntityTypeHandle                    EntityType;
            [ReadOnly] public ComponentTypeHandle<DamageData> DamageInstanceTypeRO;
            public            BufferLookup<HealthDamageBuffer>    HealthDamageBufferLookup;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var damageInstances = chunk.GetNativeArray(ref DamageInstanceTypeRO);
                
                for (var i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var damageInstance = damageInstances[i];
                    
                    if(damageInstance.Receiver.HasComponent(HealthDamageBufferLookup))
                    {
                        HealthDamageBufferLookup[damageInstance.Receiver].Add(damageInstance);
                    }
                    DestroyCommandBuffer.DestroyEntity(entity);
                }
            }
        }
        
        /// <summary>
        /// Propagates the damage from the child to the parent.
        /// </summary>
        [BurstCompile]
        private struct PropagateDamageChildToParent : IJobChunk
        {
            public            EntityTypeHandle                  EntityType;
            [ReadOnly] public ComponentTypeHandle<HealthParent> HealthParentTypeRO;
            public            ComponentTypeHandle<HealthData>   HealthDataTypeRW;
            public            BufferLookup<HealthDamageBuffer>  HealthDamageBufferLookup;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities      = chunk.GetNativeArray(EntityType);
                var healthParents = chunk.GetNativeArray(ref HealthParentTypeRO);
                var hasHealthData = chunk.Has(ref HealthDataTypeRW);
                var healthDatas    = chunk.GetNativeArray(ref HealthDataTypeRW);
                
                for (var i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var buffer = HealthDamageBufferLookup[entity];
                    var healthParentEntity = healthParents[i].Value;
                    
                    // Apply the damage to the parent
                    var healthParentDamageBuffer = HealthDamageBufferLookup[healthParentEntity];
                    // Also record this frame's damage
                    var totalDamage = 0f;
                    for (var j = 0; j < buffer.Length; j++)
                    {
                        healthParentDamageBuffer.Add(buffer[j]);
                        totalDamage += buffer[j].Value.Amount;
                    }
                    
                    //if(totalDamage>0)
                        //Debug.Log($"propagate  { entity } to {healthParentEntity} with damage {totalDamage}");

                    if (hasHealthData)
                    {
                        var healthData = healthDatas[i];
                        healthData.TakeDamage(totalDamage);
                        
                        healthDatas[i] = healthData;
                        
                        //Debug.Log($"health new value :{healthDatas[i].Value.x} and last value {healthDatas[i].Value.z}");
                    }
                    buffer.Clear();
                }
            }
        }
        
        /// <summary>
        /// Applies the damage to the health component. Todo: merge damage and apply in one go? so can be gibbed?
        /// </summary>
        [BurstCompile]
        unsafe private struct ApplyDamageJob : IJobChunk
        {
            public            uint                                  LastSystemVersion;
            [ReadOnly] public EntityTypeHandle                      EntityType;
            public            BufferTypeHandle<HealthDamageBuffer>  HealthBufferTypeHandle;
            public            ComponentTypeHandle<HealthData>       HealthTypeHandle;
            [ReadOnly] public ComponentTypeHandle<HealthMultiplier> HealthMultiplierTypeHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var    entities      = chunk.GetNativeArray(EntityType);
                var    healths       = (float3*)chunk.GetRequiredComponentDataPtrRO(ref this.HealthTypeHandle);//chunk.GetNativeArray(ref HealthTypeHandle);
                var    damageBuffers = chunk.GetBufferAccessor(ref HealthBufferTypeHandle);
                var    hasMultiplier = chunk.Has(ref HealthMultiplierTypeHandle);
                var    m             = new NativeArray<float>(chunk.Count, Allocator.Temp);
                float* multipliers   =  (float*)m.GetUnsafePtr();
                
                if (hasMultiplier)
                {
                    m.Dispose();
                    multipliers      = (float*)chunk.GetRequiredComponentDataPtrRO(ref this.HealthMultiplierTypeHandle);
                }
                
                var entityEnumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (entityEnumerator.NextEntityIndex(out var entityIndex))
                {
                    var entity       = entities[entityIndex];
                    var damageBuffer = damageBuffers[entityIndex];
                    var multiplier   = hasMultiplier ? multipliers[entityIndex] : 1.0f;
                    
                    
                    var senderEntity             = Entity.Null;
                    var damage           = 0f;
                    var perFrameDamageMultiplied = 0f;
                    var point                    = float3.zero;
                    
                    // Get sum total of all the damage in the buffer
                    for (var j = 0; j < damageBuffer.Length; j++)
                    {
                        var multipliedDamage = math.mul(damageBuffer[j].Value.Amount, multiplier);
                        perFrameDamageMultiplied += multipliedDamage;

                        senderEntity =  damageBuffer[j].Value.Sender;
                        damage       += damageBuffer[j].Value.Amount;
                        point        =  damageBuffer[j].Value.Point;
                    }
                    // Clear the damage buffer
                    damageBuffer.Clear();

                    if (chunk.IsComponentEnabled(ref HealthTypeHandle, entityIndex) && damage > 0)
                    {
                        healths[entityIndex] = new float3(healths[entityIndex].x - damage, healths[entityIndex].y, damage);
                        //Debug.Log($"health new value :{healths[entityIndex].x}");
                    }
                    
                }
                
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            /*state.Dependency = new IndexDamageJob
            {
                StreamReader         = SystemAPI.GetSingletonRW<DamageWorldSingleton>().ValueRW.GetDamageStreamReader(),
                EntityDamageIndexMap = SystemAPI.GetSingletonRW<DamageWorldSingleton>().ValueRW.DamageWorld.EntityDamageIndexMap.AsParallelWriter()
            }.Schedule(SystemAPI.GetSingletonRW<DamageWorldSingleton>().ValueRW.Count, 32, state.Dependency);*/
            state.Dependency = new EntityDamageJob
            {
                EntityDamageIndexMap = SystemAPI.GetSingletonRW<DamageWorldSingleton>().ValueRW.DamageWorld.EntityDamageIndexMap
            }.ScheduleParallel(state.Dependency);
            
            /*state.Dependency = new GatherDamageInstancesJob
            {
                HealthDamageBufferLookup = SystemAPI.GetBufferLookup<HealthDamageBuffer>(),
                DestroyCommandBuffer     = SystemAPI.GetSingletonRW<DestroyCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                EntityType               = SystemAPI.GetEntityTypeHandle(),
                DamageInstanceTypeRO     = SystemAPI.GetComponentTypeHandle<DamageEvent>(true),
            }.Schedule(collectDamageQuery, state.Dependency);*/
            
            state.Dependency = new PropagateDamageChildToParent
            {
                EntityType               = SystemAPI.GetEntityTypeHandle(),
                HealthParentTypeRO       = SystemAPI.GetComponentTypeHandle<HealthParent>(true),
                HealthDamageBufferLookup = SystemAPI.GetBufferLookup<HealthDamageBuffer>(),
                HealthDataTypeRW         = SystemAPI.GetComponentTypeHandle<HealthData>()
            }.Schedule(damageUpdateWithHealthParentQuery, state.Dependency);
            
            state.Dependency = new ApplyDamageJob
            {
                LastSystemVersion            = state.LastSystemVersion,
                EntityType                   = SystemAPI.GetEntityTypeHandle(),
                HealthTypeHandle             = SystemAPI.GetComponentTypeHandle<HealthData>(),
                HealthMultiplierTypeHandle   = SystemAPI.GetComponentTypeHandle<HealthMultiplier>(true),
                HealthBufferTypeHandle       = SystemAPI.GetBufferTypeHandle<HealthDamageBuffer>()
            }.ScheduleParallel(updatedDamageQuery, state.Dependency);
            
            state.Dependency = new ClearDamageJob
            {
                DamageWorld = SystemAPI.GetSingletonRW<DamageWorldSingleton>().ValueRW.DamageWorld
            }.Schedule(state.Dependency);
            
            //state.Dependency  = NativeStream.ScheduleConstruct(out CollisionEventDataStream, new NativeArray<int>(), state.Dependency , Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            SystemAPI.GetSingleton<DamageWorldSingleton>().Dispose();
            state.EntityManager.RemoveComponent<DamageWorldSingleton>(state.SystemHandle);
        }
    }
}