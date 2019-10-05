using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{


    
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class CreateConnectionGraphSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }



        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            /*
            
            var connectivityMapJob = new CheckConnectivityMap
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                ConnectionData = GetBufferFromEntity<Connection>(true),
                AnchoredNodeData = GetComponentDataFromEntity<StaticAnchor>(true)
            };
            var checkConnectivityHandle = connectivityMapJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(checkConnectivityHandle);

            return checkConnectivityHandle;
            */
            
            return inputDeps;
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    public class ConnectionGraphSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEntityCommandBufferSystem = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        
        
        [BurstCompile]
        private struct ClearDetachedNodes : IJobForEachWithEntity_EB<ConnectionGraph>
        {
            [ReadOnly]public ComponentDataFromEntity<BrokenNode> UnanchoredNode;
            
            public void Execute(Entity entity, int index, DynamicBuffer<ConnectionGraph> graph)
            {
                for (int i = 0; i < graph.Length; i++)
                {
                    if(UnanchoredNode.Exists(graph[i].Node))
                        graph.RemoveAt(i);
                }
            }
        }
        
        [BurstCompile]
        private struct DeleteChainJob : IJobForEachWithEntity_EBCC<GraphLink, GraphAnchor, GraphNode>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [NativeDisableParallelForRestriction] public BufferFromEntity<NodeAnchorBuffer> NodeAnchorBuffer;
            [ReadOnly]public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocity;
            public void Execute(Entity entity, int index, DynamicBuffer<GraphLink> linkBuffer, ref GraphAnchor anchorNode, ref GraphNode graphNode)
            {
                for (int i = 0; i < linkBuffer.Length; i++)
                {
                    if (PhysicsVelocity.Exists(linkBuffer[i].Node))
                    {
                        EntityCommandBuffer.DestroyEntity(index, entity);

                        if (NodeAnchorBuffer.Exists(graphNode.Node))
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

        private struct CheckConnectivityMap : IJobForEachWithEntity_EB<ConnectionGraph>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public BufferFromEntity<NodeNeighbor> Connection;
            [ReadOnly] public ComponentDataFromEntity<AnchorNode> StaticAnchor;

            public void Execute(Entity entity, int index, DynamicBuffer<ConnectionGraph> graph)
            {
                var count = 0;
                var depth = graph.Length;
                
                for (var i = 0; i < graph.Length; i++)
                {
                    var node = graph[i].Node;
                    if (!TryFindDisconnectedNodes(node, index, depth, ref count))
                    {
                        if (Connection.Exists(node))
                            EntityCommandBuffer.RemoveComponent<NodeNeighbor>(index, node);
                        Debug.Log(node + "is disconnected");
                        //Disconnect(node, index, EntityCommandBuffer);
                    }
                }

                // Destroy if no more connections exist
                if (graph.Length.Equals(0))
                    EntityCommandBuffer.DestroyEntity(index, entity);
            }

            private bool TryFindDisconnectedNodes(Entity node, int index, int depth, ref int count)
            {
                count++;
                if (count > 99)
                    return false;
                
                Debug.Log(node);
                
                if (StaticAnchor.Exists(node))
                    return true;
                if (!Connection.Exists(node))
                    return false;

                if (Connection.Exists(node))
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
                if (StaticAnchor.Exists(node))
                    return true;
                if (!Connection.Exists(node))
                    return false;

                
                if (Connection.Exists(node))
                {
                    for (var i = 0; i < Connection[node].Length; i++)
                    {
                        var subnode = Connection[node][i].Node;
                        if (StaticAnchor.Exists(subnode))
                            return true;
                        if (!Connection.Exists(subnode))
                            return false;
                    }
                }

                return false;
            }
            
            private void Disconnect(Entity node, int index, EntityCommandBuffer.Concurrent EntityCommandBuffer, int depth, ref int count)
            {
                count++;
                if (count > depth)
                    return;
                
                if (!Connection.Exists(node))
                    return;
                
                if (Connection.Exists(node))
                {
                    EntityCommandBuffer.RemoveComponent<NodeNeighbor>(index, node);

                    for (var i = 0; i < Connection[node].Length; i++)
                    {
                        Disconnect(node, index, EntityCommandBuffer, depth, ref count);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var clearJob = new ClearDetachedNodes
            {
                UnanchoredNode = GetComponentDataFromEntity<BrokenNode>(true)
            };
            var clearJobHandle = clearJob.Schedule(this, inputDeps);

            var deleteJob = new DeleteChainJob
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                NodeAnchorBuffer = GetBufferFromEntity<NodeAnchorBuffer>(),
                PhysicsVelocity = GetComponentDataFromEntity<PhysicsVelocity>(true)
            };
            var deleteJobHandle = deleteJob.Schedule(this, clearJobHandle);
            deleteJobHandle.Complete();
            
            /*
            
            var connectivityMapJob = new CheckConnectivityMap
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                ConnectionData = GetBufferFromEntity<Connection>(true),
                AnchoredNodeData = GetComponentDataFromEntity<StaticAnchor>(true)
            };
            var checkConnectivityHandle = connectivityMapJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(checkConnectivityHandle);

            return checkConnectivityHandle;
            */

            return deleteJobHandle;
        }
    }
}