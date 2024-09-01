using Junk.Health;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Junk.Fracture
{
    [DisableAutoCreation]
    public partial struct StrainEventSystem : ISystem
    {
        private EntityQuery destroyLinkEventQuery;

        public void OnCreate(ref SystemState state)
        {
            destroyLinkEventQuery = state.GetEntityQuery(typeof(DestroyLinkEvent));
        }

        [WithAll(typeof(BreakableNode))]
        [WithNone(typeof(PhysicsVelocity))]
        private partial struct FilterEventsJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Entity> DestroyLinkEventEntities;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<DestroyLinkEvent> DestroyLinkEvents;
            [ReadOnly] public ComponentLookup<AnchorNode> StaticAnchor;

            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, DynamicBuffer<NodeLinkBuffer> nodeLinkBuffer, DynamicBuffer<NodeNeighbor> neighbors)
            {
                if (nodeLinkBuffer.Length.Equals(0) && neighbors.Length.Equals(0))
                {
                    if (!StaticAnchor.HasComponent(entity))
                        EntityCommandBuffer.AddComponent(entityIndexInQuery, entity, new PhysicsVelocity());
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
                            EntityCommandBuffer.DestroyEntity(entityIndexInQuery, DestroyLinkEventEntities[k]);
                            nodeLinkBuffer.RemoveAt(i);
                        }
                    }
                }
            }
        }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new FilterEventsJob
            {
                EntityCommandBuffer = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                DestroyLinkEventEntities = destroyLinkEventQuery.ToEntityArray(Allocator.TempJob),
                DestroyLinkEvents = destroyLinkEventQuery.ToComponentDataArray<DestroyLinkEvent>(Allocator.TempJob),
                StaticAnchor = SystemAPI.GetComponentLookup<AnchorNode>(true)
            }.Schedule(state.Dependency);
        }
    }


    public partial struct StrainSystem : ISystem
    {
        private EntityQuery m_DestroyLinkEventQuery;
        
        [WithNone(typeof(PhysicsVelocity))]
        private partial struct RemoveNodeNeighbor : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [ReadOnly] public ComponentLookup<PhysicsVelocity> Velocity;
            [ReadOnly] public ComponentLookup<AnchorNode> StaticAnchor;

            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, DynamicBuffer<NodeNeighbor> neighbors)
            {
                if (neighbors.Length <= 0 && !StaticAnchor.HasComponent(entity))
                {
                    EntityCommandBuffer.AddComponent<PhysicsVelocity>(entityIndexInQuery, entity);
                    EntityCommandBuffer.RemoveComponent<NodeNeighbor>(entityIndexInQuery, entity);
                    return;
                }

                for (var i = neighbors.Length - 1; i > -1; i--)
                {
                    if (Velocity.HasComponent(neighbors[i].Node)) //neighbors[i].Node.Equals(Entity.Null) && 
                    {
                        neighbors.RemoveAt(i);
                    }
                }
            }
        }

        [WithNone(typeof(PhysicsVelocity))]
        private partial struct UnanchorNodeJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [ReadOnly] public ComponentLookup<AnchorNode> StaticAnchor;
            [ReadOnly] public ComponentLookup<PhysicsVelocity> PhysicsVelocity;

            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, DynamicBuffer<NodeAnchorBuffer> b)
            {
                if (b.Length.Equals(0))
                {
                    EntityCommandBuffer.AddComponent<PhysicsVelocity>(entityIndexInQuery, entity);
                    EntityCommandBuffer.RemoveComponent<NodeAnchorBuffer>(entityIndexInQuery, entity);
                }

                for (var i = b.Length - 1; i > -1; i--)
                {
                    if (StaticAnchor.HasComponent(b[i].Node) && PhysicsVelocity.HasComponent(b[i].Node)
                    ) //neighbors[i].Node.Equals(Entity.Null) && 
                    {
                        b.RemoveAt(i);
                    }
                }
            }
        }

        [WithAll(typeof(BreakableNode))]
        [WithNone(typeof(PhysicsVelocity))]
        private partial struct CheckHealth : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, ref HealthData health)
            {
                if (health.Value.x <= 0)
                {
                    EntityCommandBuffer.RemoveComponent(entityIndexInQuery, entity, typeof(BreakableNode));
                    EntityCommandBuffer.AddComponent(entityIndexInQuery, entity, new PhysicsVelocity());
                    EntityCommandBuffer.AddComponent(entityIndexInQuery, entity, new BrokenNode());
                }
            }
        }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new RemoveNodeNeighbor
            {
                EntityCommandBuffer = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                Velocity = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
                StaticAnchor = SystemAPI.GetComponentLookup<AnchorNode>(true)
            }.Schedule(state.Dependency);

            state.Dependency = new UnanchorNodeJob
            {
                EntityCommandBuffer = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                StaticAnchor = SystemAPI.GetComponentLookup<AnchorNode>(true),
                PhysicsVelocity = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
            }.Schedule(state.Dependency);

            state.Dependency = new CheckHealth 
            {
                EntityCommandBuffer = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.Schedule(state.Dependency);
        }
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