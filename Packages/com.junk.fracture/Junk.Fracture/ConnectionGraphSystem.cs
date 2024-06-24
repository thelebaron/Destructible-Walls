using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Junk.Fracture
{
    public partial struct ConnectionGraphSystem : ISystem
    {
        [BurstCompile]
        private partial struct ClearDetachedNodes : IJobEntity
        {
            [ReadOnly]public ComponentLookup<BrokenNode> UnanchoredNode;
            
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, DynamicBuffer<ConnectionGraph> graph)
            {
                for (int i = 0; i < graph.Length; i++)
                {
                    if(UnanchoredNode.HasComponent(graph[i].Node))
                        graph.RemoveAt(i);
                }
            }
        }
        
        //[BurstCompile]
        private partial struct DestroyLinkJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [NativeDisableParallelForRestriction] public BufferLookup<NodeAnchorBuffer> NodeAnchorBuffer;
            [ReadOnly]public ComponentLookup<PhysicsVelocity> PhysicsVelocity;
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, DynamicBuffer<GraphLink> linkBuffer, ref GraphAnchor anchorNode, ref GraphNode graphNode)
            {
                for (int i = 0; i < linkBuffer.Length; i++)
                {
                    if (PhysicsVelocity.HasComponent(linkBuffer[i].Node))
                    {
                        EntityCommandBuffer.DestroyEntity(entityIndexInQuery, entity);

                        var evententity = EntityCommandBuffer.CreateEntity(entityIndexInQuery);
                        EntityCommandBuffer.AddComponent(entityIndexInQuery, evententity, new DestroyLinkEvent
                        {
                            DestroyedLink = entity
                        });
                        

                        if (NodeAnchorBuffer.HasBuffer(graphNode.Node))
                        {
                            var nodeAnchorBuffer = NodeAnchorBuffer[graphNode.Node];
                            
                            for(var k = nodeAnchorBuffer.Length - 1; k > -1; k--)
                            {
                                if (anchorNode.Node.Equals(nodeAnchorBuffer[k].Node))
                                {
                                    nodeAnchorBuffer.RemoveAt(k);
                                }
                            }
                            
                            
                        }
                        // Also send ecb to remove anchor from Node's buffer of anchors
                        /*
                         Node
                         *
                         * 
                         */
                    }
                }
            }
        }

        private partial struct CheckConnectivityMap : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [ReadOnly] public BufferLookup<NodeNeighbor> Connection;
            [ReadOnly] public ComponentLookup<AnchorNode> StaticAnchor;

            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndexInQuery, DynamicBuffer<ConnectionGraph> graph)
            {
                var count = 0;
                var depth = graph.Length;
                
                for (var i = 0; i < graph.Length; i++)
                {
                    var node = graph[i].Node;
                    if (!TryFindDisconnectedNodes(node, entityIndexInQuery, depth, ref count))
                    {
                        if (Connection.HasBuffer(node))
                            EntityCommandBuffer.RemoveComponent<NodeNeighbor>(entityIndexInQuery, node);
                        Debug.Log(node + "is disconnected");
                        //Disconnect(node, index, EntityCommandBuffer);
                    }
                }

                // Destroy if no more connections exist
                if (graph.Length.Equals(0))
                    EntityCommandBuffer.DestroyEntity(entityIndexInQuery, entity);
            }

            private bool TryFindDisconnectedNodes(Entity node, int index, int depth, ref int count)
            {
                count++;
                if (count > 99)
                    return false;
                
                Debug.Log(node);
                
                if (StaticAnchor.HasComponent(node))
                    return true;
                if (!Connection.HasBuffer(node))
                    return false;

                if (Connection.HasBuffer(node))
                {
                    for (var i = 0; i < Connection[node].Length; i++)
                    {
                        return TryFindDisconnectedNodes(node, index, depth, ref count);
                    }
                }

                return false;
            }

            private bool FindAnchorNode2(Entity node, int index, int depth, ref int count)
            {
                if (StaticAnchor.HasComponent(node))
                    return true;
                if (!Connection.HasBuffer(node))
                    return false;

                
                if (Connection.HasBuffer(node))
                {
                    for (var i = 0; i < Connection[node].Length; i++)
                    {
                        var subnode = Connection[node][i].Node;
                        if (StaticAnchor.HasComponent(subnode))
                            return true;
                        if (!Connection.HasComponent(subnode))
                            return false;
                    }
                }

                return false;
            }
            
            private void Disconnect(Entity node, int index, EntityCommandBuffer.ParallelWriter EntityCommandBuffer, int depth, ref int count)
            {
                count++;
                if (count > depth)
                    return;
                
                if (!Connection.HasBuffer(node))
                    return;
                
                if (Connection.HasBuffer(node))
                {
                    EntityCommandBuffer.RemoveComponent<NodeNeighbor>(index, node);

                    for (var i = 0; i < Connection[node].Length; i++)
                    {
                        Disconnect(node, index, EntityCommandBuffer, depth, ref count);
                    }
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new ClearDetachedNodes
            {
                UnanchoredNode = SystemAPI.GetComponentLookup<BrokenNode>(true)
            }.Schedule(state.Dependency);

            state.Dependency = new DestroyLinkJob
            {
                EntityCommandBuffer = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                NodeAnchorBuffer    = SystemAPI.GetBufferLookup<NodeAnchorBuffer>(),
                PhysicsVelocity     = SystemAPI.GetComponentLookup<PhysicsVelocity>(true)
            }.Schedule(state.Dependency);
            state.Dependency.Complete();
        }
    }
}