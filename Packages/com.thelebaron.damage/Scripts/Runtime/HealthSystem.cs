using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Profiling;

namespace thelebaron.damage
{
    public class HealthSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
        //private EntityQuery                            damageEventsQuery;
        private EntityQuery                            historyQuery;

        protected override void OnCreate()
        {
            endSimulationEntityCommandBufferSystem            = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            //damageEventsQuery = GetEntityQuery(typeof(DamageEvent));
            //historyQuery      = GetEntityQuery(typeof(DamageHistory));
        }

        /*
        /// <summary>
        /// Record a history of all damage events that occured.
        /// </summary>
        [BurstCompile]
        private struct HistoryJob : IJobChunk
        {
            [ReadOnly] public float                    Time;
            [ReadOnly][DeallocateOnJobCompletion] public NativeArray<DamageEvent> DamageEvents;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public ArchetypeChunkBufferType<DamageHistory> DamageHistoryType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkEntity    = chunk.GetNativeArray(EntityType);
                var chunkHistories = chunk.GetBufferAccessor(DamageHistoryType);

                for (int index = 0; index < chunkEntity.Length; index++)
                {
                    var entity  = chunkEntity[index];
                    var history = chunkHistories[index];

                    for (var i = 0; i < DamageEvents.Length; i++)
                    {
                        if (entity.Equals(DamageEvents[i].Receiver))
                        {
                            var de = DamageEvents[i];

                            var dh = new DamageHistory
                            {
                                TimeOccured     = Time,
                                TookDamage      = true,
                                Damage          = de.Amount,
                                Instigator      = de.Sender,
                                LastDamageEvent = de
                            };

                            history.Add(dh);
                        }
                    }
                }
            }
        }
        */
        
        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        [ExcludeComponent(typeof(Dead))]
        private struct AddDamageStackJob : IJobForEachWithEntity<DamageEvent>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [NativeDisableParallelForRestriction] public BufferFromEntity<DamageStack> DamageStackBuffer;
            
            public void Execute(Entity entity, int index, ref  DamageEvent damageEvent)
            {
                if (DamageStackBuffer.Exists(damageEvent.Receiver))
                {
                    DamageStackBuffer[damageEvent.Receiver].Add(damageEvent);
                }
                // Destroy damage event entity
                EntityCommandBuffer.DestroyEntity(index, entity);
            }
        }

        /// <summary>
        /// Applies the damage to the health component. Todo: merge damage and apply in one go? so can be gibbed?
        /// </summary>
        [BurstCompile]
        [ExcludeComponent(typeof(Dead))]
        private struct ApplyDamageJob : IJobForEachWithEntity_EBC<DamageStack, Health>
        {
            public void Execute(Entity entity, int index, DynamicBuffer<DamageStack> stacks, ref Health health)
            {
                if(stacks.Length<1)
                    return;
                
                var damagetotal = new DamageEvent();
                var sender = Entity.Null;
                
                // Apply damage
                for (var de = 0; de < stacks.Length; de++)
                {
                    sender = stacks[de].Value.Sender;
                    damagetotal.Amount += stacks[de].Value.Amount;
                }
                
                health.ApplyDamage(damagetotal);
                health.Damager = sender;
                health.DamageTaken = damagetotal.Amount;
                stacks.Clear();
                
                
            }
        }
        
        protected override void OnUpdate()
        {
            /*
            Profiler.BeginSample("Health History");
            var entityType        = GetArchetypeChunkEntityType();
            var damagehistoryType = GetArchetypeChunkBufferType<DamageHistory>();
            
            var historyJob = new HistoryJob
            {
                Time              = (float)Time.ElapsedTime,
                DamageEvents      = damageEventsQuery.ToComponentDataArray<DamageEvent>(Allocator.TempJob),
                EntityType        = entityType,
                DamageHistoryType = damagehistoryType
            };
            var historyHandle = historyJob.Schedule(historyQuery, inputDeps);
            Profiler.EndSample();*/

            Profiler.BeginSample("Health AddDamageStack");
            Dependency = new AddDamageStackJob
            {
                EntityCommandBuffer               = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                DamageStackBuffer = GetBufferFromEntity<DamageStack>()
            }.Schedule(this, Dependency);
            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            Profiler.EndSample();
            
            Profiler.BeginSample("Health ApplyDamageJob");
            Dependency = new ApplyDamageJob().Schedule(this, Dependency);
            Profiler.EndSample();
            
            //var tagDeadJob = new TagDeadJob{ EntityCommandBuffer = m_EndSim.CreateCommandBuffer().ToConcurrent()};
            //var tagDeadHandle = tagDeadJob.Schedule(this, processDamageStackHandle);
            //m_EndSim.AddJobHandleForProducer(tagDeadHandle);
            //return tagDeadHandle;
            //return processDamageStackHandle;
        }
    }
}