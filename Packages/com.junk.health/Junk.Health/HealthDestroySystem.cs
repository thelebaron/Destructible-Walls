using Junk.Entities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Junk.Hitpoints
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(HealthSystem))]
    public partial struct HealthDestroySystem : ISystem
    {   
        private EntityQuery                            destroyQuery;
         
        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        private struct DestroyEntityJob : IJobChunk
        {
            public            EntityCommandBuffer CommandBuffer;
            [ReadOnly] public EntityTypeHandle                   EntityType;
            [ReadOnly] public ComponentTypeHandle<HealthData>        HealthType;   
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var healths  = chunk.GetNativeArray(ref HealthType);
                
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var health = healths[i];
                    
                    if (health.Value<=0)
                        CommandBuffer.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnCreate(ref  SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<HealthData, DestroyOnZeroHealth>();
            destroyQuery = state.GetEntityQuery(builder);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            state.Dependency = new DestroyEntityJob
            {
                CommandBuffer = SystemAPI.GetSingleton<DestroyCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
                EntityType    = SystemAPI.GetEntityTypeHandle(),
                HealthType    = SystemAPI.GetComponentTypeHandle<HealthData>(true)
            }.Schedule(destroyQuery, state.Dependency);
        }
    }
}