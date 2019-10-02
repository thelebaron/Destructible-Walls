using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Destructibles
{
    /// <summary>
    /// A graph entity is created for each entity which has a StaticAnchor component.
    /// </summary>
    public struct ConnectionGraph : IBufferElementData
    {
        /// <summary>
        /// A node entity.
        /// </summary>
        public Entity RootNode;

        /// <summary>
        /// Provides implicit conversion of an <see cref="Entity"/> to a Node element.
        /// </summary>
        /// <param name="e">The entity to convert</param>
        /// <returns>A new buffer element.</returns>
        public static implicit operator ConnectionGraph(Entity e)
        {
            return new ConnectionGraph {RootNode = e};
        }
    }

    
    
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
        
        

        private struct CheckConnectivityMap : IJobForEachWithEntity_EB<ConnectionGraph>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public BufferFromEntity<Connection> Connection;
            [ReadOnly] public ComponentDataFromEntity<StaticAnchor> StaticAnchor;

            public void Execute(Entity entity, int index, DynamicBuffer<ConnectionGraph> graph)
            {
                var count = 0;
                var depth = graph.Length;
                
                for (var i = 0; i < graph.Length; i++)
                {
                    var node = graph[i].RootNode;
                    if (!TryFindDisconnectedNodes(node, index, depth, ref count))
                    {
                        if (Connection.Exists(node))
                            EntityCommandBuffer.RemoveComponent<Connection>(index, node);
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
                    EntityCommandBuffer.RemoveComponent<Connection>(index, node);

                    for (var i = 0; i < Connection[node].Length; i++)
                    {
                        Disconnect(node, index, EntityCommandBuffer, depth, ref count);
                    }
                }
            }
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
}