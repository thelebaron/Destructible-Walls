using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Hitpoints
{
    /// <summary>
    /// End frame reset the lastframe damage parameter of health.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct LateHealthSystem : ISystem
    {
        private EntityQuery healthChangedQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Query for damage buffer changes to apply to Health
            healthChangedQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<HealthData>()
                .Build(ref state);
            healthChangedQuery.SetChangedVersionFilter(ComponentType.ReadWrite<HealthData>());
        }
        
        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        private struct ClearLastFrameDamage : IJobChunk
        {
            public ComponentTypeHandle<HealthData> HealthDataTypeRW;
            
            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var healths          = (float3*)chunk.GetRequiredComponentDataPtrRW(ref this.HealthDataTypeRW);
                var entityEnumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (entityEnumerator.NextEntityIndex(out var entityIndex))
                {
                    var damaged = healths[entityIndex].z > 0;
                    
                    // Only change if last damage isn't 0, otherwise this apparently always overwrites
                    if (chunk.IsComponentEnabled(ref HealthDataTypeRW, entityIndex) && damaged)
                        healths[entityIndex] = new float3(healths[entityIndex].x, healths[entityIndex].y, 0);
                    
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ClearLastFrameDamage
            {
                HealthDataTypeRW = SystemAPI.GetComponentTypeHandle<HealthData>()
            }.Schedule(healthChangedQuery, state.Dependency);
        }
    }
}