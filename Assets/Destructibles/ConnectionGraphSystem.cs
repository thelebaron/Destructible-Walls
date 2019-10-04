using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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

        private struct ProcessEvents : IJobForEachWithEntity<BreakEvent>
        {
            public ComponentDataFromEntity<Anchored> AnchoredNode;
            public ComponentDataFromEntity<NodeParent> NodeParent;
            public BufferFromEntity<NodeChild> NodeChild;

            public void Execute(Entity entity, int index, ref BreakEvent breakEvent)
            {
                
                
                if (NodeChild.Exists(breakEvent.NodeEntity))
                {
                    Recurse(breakEvent.NodeEntity, NodeChild[breakEvent.NodeEntity]);
                    
                    
                }
            }

            private void Recurse(Entity entity, DynamicBuffer<NodeChild>dynamicBuffer)
            {
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                        //if(dynamicBuffer[i].Node)
                }
            }
        }

        [BurstCompile]
        private struct ClearDetachedNodes : IJobForEachWithEntity_EB<ConnectionGraph>
        {
            [ReadOnly]public ComponentDataFromEntity<Unanchored> UnanchoredNode;
            
            public void Execute(Entity entity, int index, DynamicBuffer<ConnectionGraph> graph)
            {
                for (int i = 0; i < graph.Length; i++)
                {
                    if(UnanchoredNode.Exists(graph[i].Node))
                        graph.RemoveAt(i);
                }
            }
        }

        private struct CheckConnectivityMap : IJobForEachWithEntity_EB<ConnectionGraph>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public BufferFromEntity<Neighbors> Connection;
            [ReadOnly] public ComponentDataFromEntity<StaticAnchor> StaticAnchor;

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
                            EntityCommandBuffer.RemoveComponent<Neighbors>(index, node);
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
                    EntityCommandBuffer.RemoveComponent<Neighbors>(index, node);

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
                UnanchoredNode = GetComponentDataFromEntity<Unanchored>(true)
            };
            
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
            
            return clearJob.Schedule(this, inputDeps);
        }
    }
}