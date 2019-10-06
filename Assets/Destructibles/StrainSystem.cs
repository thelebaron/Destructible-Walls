using thelebaron.Damage;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{
    public class StrainEventSystem : JobComponentSystem
    {
        private EntityQuery m_DestroyLinkEventQuery;
        private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_DestroyLinkEventQuery = GetEntityQuery(typeof(DestroyLinkEvent));
        }

        [RequireComponentTag(typeof(BreakableNode))]
        [ExcludeComponent(typeof(PhysicsVelocity))]
        private struct FilterEventsJob : IJobForEachWithEntity_EBB<NodeLinkBuffer, NodeNeighbor>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Entity> DestroyLinkEventEntities;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<DestroyLinkEvent> DestroyLinkEvents;
            [ReadOnly] public ComponentDataFromEntity<AnchorNode> StaticAnchor;

            public void Execute(Entity entity, int index, DynamicBuffer<NodeLinkBuffer> nodeLinkBuffer, DynamicBuffer<NodeNeighbor> neighbors)
            {
                
                if (nodeLinkBuffer.Length.Equals(0) && neighbors.Length.Equals(0))
                {
                    if (!StaticAnchor.Exists(entity))
                        EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
                    
                    return;
                }
                
                // Go through all buffer entities for a match
                if (nodeLinkBuffer.Length.Equals(0) || DestroyLinkEvents.Length.Equals(0))
                    return;
                
                
                // Go through all buffer entities for a match
                for (var i = nodeLinkBuffer.Length - 1; i > -1; i--)
                {
                    for (int k = 0; k < DestroyLinkEvents.Length; k++)
                    {
                        if (DestroyLinkEvents[k].DestroyedLink.Equals(nodeLinkBuffer[i].Link))
                        {
                            // Destroy the event entity
                            EntityCommandBuffer.DestroyEntity(index, DestroyLinkEventEntities[k]);
                            nodeLinkBuffer.RemoveAt(i);
                        }
                    }
                }

            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var filtereventsjob = new FilterEventsJob
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                DestroyLinkEventEntities = m_DestroyLinkEventQuery.ToEntityArray(Allocator.TempJob),
                DestroyLinkEvents = m_DestroyLinkEventQuery.ToComponentDataArray<DestroyLinkEvent>(Allocator.TempJob),
                StaticAnchor = GetComponentDataFromEntity<AnchorNode>(true)
            };
            var filtereventsHandle = filtereventsjob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(filtereventsHandle);

            return filtereventsHandle;
        }
    }


    public class StrainSystem : JobComponentSystem
    {
        private EntityQuery m_DestroyLinkEventQuery;


        [ExcludeComponent(typeof(PhysicsVelocity))]
        private struct RemoveNodeNeighbor : IJobForEachWithEntity_EB<NodeNeighbor>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public ComponentDataFromEntity<PhysicsVelocity> Velocity;
            [ReadOnly] public ComponentDataFromEntity<AnchorNode> StaticAnchor;

            public void Execute(Entity entity, int index, DynamicBuffer<NodeNeighbor> neighbors)
            {
                if (neighbors.Length <= 0 && !StaticAnchor.Exists(entity))
                {
                    EntityCommandBuffer.AddComponent<PhysicsVelocity>(index, entity);
                    EntityCommandBuffer.RemoveComponent<NodeNeighbor>(index, entity);
                    return;
                }

                for (var i = neighbors.Length - 1; i > -1; i--)
                {
                    if (Velocity.Exists(neighbors[i].Node)) //neighbors[i].Node.Equals(Entity.Null) && 
                    {
                        neighbors.RemoveAt(i);
                    }
                }
            }
        }

        [ExcludeComponent(typeof(PhysicsVelocity))]
        private struct UnanchorNodeJob : IJobForEachWithEntity_EB<NodeAnchorBuffer>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public ComponentDataFromEntity<AnchorNode> StaticAnchor;
            [ReadOnly] public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocity;

            public void Execute(Entity entity, int index, DynamicBuffer<NodeAnchorBuffer> b)
            {
                if (b.Length.Equals(0))
                {
                    EntityCommandBuffer.AddComponent<PhysicsVelocity>(index, entity);
                    EntityCommandBuffer.RemoveComponent<NodeAnchorBuffer>(index, entity);
                }

                for (var i = b.Length - 1; i > -1; i--)
                {
                    if (StaticAnchor.Exists(b[i].Node) && PhysicsVelocity.Exists(b[i].Node)
                    ) //neighbors[i].Node.Equals(Entity.Null) && 
                    {
                        b.RemoveAt(i);
                    }
                }
            }
        }

        [RequireComponentTag(typeof(BreakableNode))]
        [ExcludeComponent(typeof(PhysicsVelocity))]
        private struct CheckHealth : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;

            public void Execute(Entity entity, int index, ref Health health)
            {
                if (health.Value <= 0)
                {
                    EntityCommandBuffer.RemoveComponent(index, entity, typeof(BreakableNode));
                    EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
                    EntityCommandBuffer.AddComponent(index, entity, new BrokenNode());
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var removeNeighborsJob = new RemoveNodeNeighbor
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Velocity = GetComponentDataFromEntity<PhysicsVelocity>(true),
                StaticAnchor = GetComponentDataFromEntity<AnchorNode>(true)
            };
            var removeNeighborsHandle = removeNeighborsJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(removeNeighborsHandle);

            var unanchorJob = new UnanchorNodeJob
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                StaticAnchor = GetComponentDataFromEntity<AnchorNode>(true),
                PhysicsVelocity = GetComponentDataFromEntity<PhysicsVelocity>(true),
            };
            var unanchorJobHandle = unanchorJob.Schedule(this, removeNeighborsHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(unanchorJobHandle);

            var checkHealthJob = new CheckHealth
                {EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()};
            var checkHealthHandle = checkHealthJob.Schedule(this, unanchorJobHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(checkHealthHandle);

            return checkHealthHandle;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
    }
}

/*        
/// <summary>
/// If a node has no immediate anchor, detach it. For now set health value so we can burst this job.
/// </summary>
[BurstCompile]
[RequireComponentTag(typeof(Node), typeof(DynamicAnchor))]
[ExcludeComponent(typeof(PhysicsVelocity),typeof(StaticAnchor))]
struct CheckDynamicAnchors : IJobForEachWithEntity_EBC<Connection, Health>
{
    public EntityCommandBuffer.Concurrent EntityCommandBuffer;
    [ReadOnly] public ComponentDataFromEntity<DynamicAnchor> DynamicAnchorData;
    
    public void Execute(Entity entity, int index, DynamicBuffer<Connection> connection, ref Health health)
    {
        bool removeAnchor = true;
        
        for (int i = 0; i < connection.Length; i++)
        {
            if (DynamicAnchorData.Exists(connection[i].Node))
            {
                removeAnchor = false;
            }
        }

        if (removeAnchor)
            health.Value = 0; //EntityCommandBuffer.RemoveComponent(index, entity, typeof(DynamicAnchor));
    }
}*/