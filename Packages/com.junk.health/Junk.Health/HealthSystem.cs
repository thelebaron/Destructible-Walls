using Junk.Entities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
#pragma warning disable CS0282 // There is no defined ordering between fields in multiple declarations of partial struct

namespace Junk.Hitpoints
{
    public class DamageData : IComponentData
    {
        public NativeStream PendingStream;
    }
    
    /// <summary>
    /// See TRSToLocalToWorldSystem / TRSToLocalToParentSystem
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct HealthSystem : ISystem
    {
        private EntityQuery healthQuery;        
        private EntityQuery destroyQuery;   
        private EntityQuery damageMessageQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<HealthDamageBuffer>()
                .WithAnyRW<HealthData, HealthState>()
                .WithAnyRW<HealthPhysicsDeath, HealthFeedback>()
                .WithAny<HealthParent, HealthMultiplier>()
                .WithNone<Dead>()
                .WithOptions(EntityQueryOptions.FilterWriteGroup);
            healthQuery = state.GetEntityQuery(builder);
                
            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<HealthData, DestroyOnZeroHealth>()
                .WithOptions(EntityQueryOptions.Default);
            destroyQuery = state.GetEntityQuery(builder);
            
            builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DamageInstance>()
                .WithOptions(EntityQueryOptions.Default);
            damageMessageQuery = state.GetEntityQuery(builder);
        }


        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        private partial struct ProcessDamageEventsJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public BufferLookup<HealthDamageBuffer>         DamageEventBuffer;
            public                                       EntityCommandBuffer DestroyCommandBuffer;
                
            public void Execute(Entity entity, DamageInstance damageInstance)
            {
                if(damageInstance.Receiver.HasComponent(DamageEventBuffer))
                {
                    DamageEventBuffer[damageInstance.Receiver].Add(damageInstance);
                }
                DestroyCommandBuffer.DestroyEntity(entity);
            }
        }
        

        /// <summary>
        /// Applies the damage to the health component. Todo: merge damage and apply in one go? so can be gibbed?
        /// </summary>
        [BurstCompile]
        private struct ApplyHealthJob : IJobChunk
        {
            public            EntityCommandBuffer.ParallelWriter      CommandBuffer;
            public            float                                   ElapsedTime;
            [ReadOnly] public EntityTypeHandle                        EntityType;
            public            ComponentTypeHandle<HealthData>             HealthTypeHandle;
            [ReadOnly] public ComponentTypeHandle<HealthMultiplier>   HealthMultiplierTypeHandle;
            [ReadOnly] public ComponentTypeHandle<HealthParent>       ParentHealthTypeHandle;
            public            ComponentTypeHandle<HealthPhysicsDeath> HealthPhysicsDeathTypeHandle;
            public            BufferTypeHandle<HealthDamageBuffer>    HealthBufferTypeHandle;
            public            ComponentTypeHandle<HealthState>        HealthStateTypeHandle;
            public            ComponentTypeHandle<HealthFeedback>     HealthFeedbackTypeHandle;
            [ReadOnly] public ComponentLookup<LocalToWorld>           LocalToWorldFromEntity;
            public            uint                                    LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                // note if unchanged, feedback isnt written to and stores last result, resulting in endless feedback
                /*var changed = chunk.DidOrderChange(LastSystemVersion) || chunk.DidChange(HealthBufferTypeHandle, LastSystemVersion);
                if (!changed)
                    return;*/

                var entities            = chunk.GetNativeArray(EntityType);
                var healths             = chunk.GetNativeArray(ref HealthTypeHandle);
                var damageBuffers       = chunk.GetBufferAccessor(ref HealthBufferTypeHandle);
                var healthStates        = chunk.GetNativeArray(ref HealthStateTypeHandle);
                var healthParents       = chunk.GetNativeArray(ref ParentHealthTypeHandle);
                var healthMultipliers   = chunk.GetNativeArray(ref HealthMultiplierTypeHandle);
                var healthPhysicsDeaths = chunk.GetNativeArray(ref HealthPhysicsDeathTypeHandle);
                var healthFeedbacks     = chunk.GetNativeArray(ref HealthFeedbackTypeHandle);
                var hasHealth           = chunk.Has(ref HealthTypeHandle);
                var hasParentHealth     = chunk.Has(ref ParentHealthTypeHandle);
                var hasMultiplier       = chunk.Has(ref HealthMultiplierTypeHandle);
                var hasState            = chunk.Has(ref HealthStateTypeHandle);
                var hasPhysicsDeath     = chunk.Has(ref HealthPhysicsDeathTypeHandle);
                var hasFeedback         = chunk.Has(ref HealthFeedbackTypeHandle);
                var count               = chunk.Count;
                
                if (hasHealth)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var entity = entities[i];
                        var health      = healths[i];
                        var damageBuffer = damageBuffers[i];
                        var multiplier  = 1f;
                        if(hasMultiplier)
                            multiplier = healthMultipliers[i].Value;
                        
                        //var soloHealth = health.Value <= 0 && !hasParentHealth;
                        var pendingDamage = damageBuffer.Length > 0;


                        // Skip if dead or no events in buffer
                        //if(soloHealth/* || damageBuffer.Length < 1*/)
                            //continue;
                        
                        Entity senderEntity   = default;
                        var    perFrameDamage = 0f;
                        var    perFrameDamageMultiplied = 0f;
                        var    lastDamage     = 0f;
                        var    point          = new float3(0f);
                        
                        // Get sum total of all the damage in the buffer
                        for (var j = 0; j < damageBuffer.Length; j++)
                        {
                            var multipliedDamage = math.mul(damageBuffer[j].Value.Amount, multiplier);
                            perFrameDamageMultiplied += multipliedDamage;

                            senderEntity   =  damageBuffer[j].Value.Sender;
                            lastDamage     =  damageBuffer[j].Value.Amount;
                            perFrameDamage += damageBuffer[j].Value.Amount;
                            point          =  damageBuffer[j].Value.Point;
                        }
                        
                        if (hasParentHealth && pendingDamage)
                        {
                            var parentHealth  = healthParents[i].Value;
                            var damageMessage = new DamageInstance
                            {
                                Amount   = perFrameDamageMultiplied,
                                Receiver = parentHealth,
                                Sender   = senderEntity,
                                Point    = point,
                                CreatedBy = entity
                            };
                            // Write the new event
                            var newDamageEntity = CommandBuffer.CreateEntity(unfilteredChunkIndex);
                            CommandBuffer.AddComponent(unfilteredChunkIndex, newDamageEntity, damageMessage);
                        }
                        
                        
                        if (hasPhysicsDeath && pendingDamage)
                        {
                            var direction = LocalToWorldFromEntity[entity].Position - LocalToWorldFromEntity[senderEntity].Position ;
                            healthPhysicsDeaths[i]= new HealthPhysicsDeath
                            {
                                Force = perFrameDamage,
                                Direction = direction,
                                Point = point
                            };
                        }
                        
                        if (hasFeedback)
                        {
                            healthFeedbacks[i] = new HealthFeedback
                            {
                                LastFrameDamage = perFrameDamage
                                //Value = point
                            };
                        }
                        
                        // Clear the damage buffer
                        damageBuffer.Clear();
                        
                        // Update info(deprecated for below)
                        //health.LastDamageValue   = lastDamage;
                        //health.LastDamagerEntity = senderEntity;
                        
                        // this could be moved to another job specifically querying for the state component on a per chunk basis
                        if (hasState)
                        {
                            // Update per frame changes from damage events
                            var state = healthStates[i];                            
                            // Zero out damage, a bit hacky but should work without needing complexity
                            if (state.Invulnerable)
                                perFrameDamage = 0;
                            state.LastDamagerEntity = senderEntity;
                            state.LastDamageValue   = lastDamage;
                            state.TimeLastHurt      = ElapsedTime;
                            state.TotalDamage       += perFrameDamage;
                            
                            healthStates[i] = state;
                        }
                        
                        // Subtract total damage from health
                        health.Value -= perFrameDamage;
                        healths[i] = health;
                    }
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ProcessDamageEventsJob
            {
                DamageEventBuffer = SystemAPI.GetBufferLookup<HealthDamageBuffer>(),
                DestroyCommandBuffer = SystemAPI.GetSingletonRW<DestroyCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),

            }.Schedule(state.Dependency);
            
            state.Dependency = new ApplyHealthJob
            {
                CommandBuffer                = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                ElapsedTime                  = (float)SystemAPI.Time.ElapsedTime,
                EntityType                   = SystemAPI.GetEntityTypeHandle(),
                HealthTypeHandle             = SystemAPI.GetComponentTypeHandle<HealthData>(),
                HealthMultiplierTypeHandle   = SystemAPI.GetComponentTypeHandle<HealthMultiplier>(true),
                ParentHealthTypeHandle       = SystemAPI.GetComponentTypeHandle<HealthParent>(true),
                HealthPhysicsDeathTypeHandle = SystemAPI.GetComponentTypeHandle<HealthPhysicsDeath>(),
                HealthBufferTypeHandle       = SystemAPI.GetBufferTypeHandle<HealthDamageBuffer>(),
                HealthStateTypeHandle        = SystemAPI.GetComponentTypeHandle<HealthState>(),
                HealthFeedbackTypeHandle     = SystemAPI.GetComponentTypeHandle<HealthFeedback>(),
                LocalToWorldFromEntity       = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                LastSystemVersion            = state.LastSystemVersion
            }.ScheduleParallel(healthQuery, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {}
    }
}