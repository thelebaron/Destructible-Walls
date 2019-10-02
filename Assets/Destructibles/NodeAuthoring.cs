using System.Collections.Generic;
using System.Linq;
using thelebaron.Damage;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{
    public struct Node : IComponentData
    {
    }

    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        private GameObject RootGameObject;
        public List<Transform> Connections = new List<Transform>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var rootEntity = conversionSystem.GetPrimaryEntity(transform.parent);
            var connectionGraph = dstManager.GetBuffer<ConnectionGraph>(rootEntity);
            connectionGraph.Add(entity);

            // Add the node buffer
            dstManager.AddComponentData(entity, new Health {Value = 10, Max = 10});
            dstManager.AddComponentData(entity, new Node());
            dstManager.AddComponentData(entity, new DynamicAnchor());

            dstManager.SetName(entity, "FractureNode_" + name);

            var connectionJoints = dstManager.AddBuffer<Connection>(entity);
            for (int i = 0; i < Connections.Count; i++)
            {
                var otherentity = conversionSystem.GetPrimaryEntity(Connections[i].gameObject);

                connectionJoints.Add(otherentity);
            }

            /*
            foreach (var node in Connections)
            {
                var otherentity = conversionSystem.GetPrimaryEntity(node.gameObject);

                // Do stuff I dont understand..
                RigidTransform bFromA = math.mul(math.inverse(worldFromB(node)), worldFromA);
                var PositionInConnectedEntity = float3.zero;
                var OrientationInConnectedEntity = quaternion.identity;
                var PositionLocal    = math.transform(bFromA, PositionInConnectedEntity);
                var OrientationLocal = math.mul(bFromA.rot, OrientationInConnectedEntity);
                
                
                var jointData = JointData.CreateFixed(PositionLocal, PositionInConnectedEntity, OrientationLocal, OrientationInConnectedEntity);
                PhysicsBaseMethods.CreateJoint(jointData, entity, otherentity, dstManager);
            }*/
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                referencedPrefabs.Add(Connections[i].gameObject);
            }
        }

        public RigidTransform worldFromA =>
            new RigidTransform(gameObject.transform.rotation, gameObject.transform.position);

        public RigidTransform worldFromB(Transform tr)
        {
            return new RigidTransform(tr.rotation, tr.position);
        }
    }

    /// <summary>
    /// A connection joint contains only the immediate entities which are connected to a node.
    /// </summary>
    public struct Connection : IBufferElementData
    {
        /// <summary>
        /// A node entity.
        /// </summary>
        public Entity Node;

        /// <summary>
        /// Provides implicit conversion of an <see cref="Entity"/> to a Node element.
        /// </summary>
        /// <param name="e">The entity to convert</param>
        /// <returns>A new buffer element.</returns>
        public static implicit operator Connection(Entity e)
        {
            return new Connection {Node = e};
        }
    }

    /// <summary>
    /// A node contains all other entities connected to the current node.
    /// </summary>
    public struct ConnectionGraph : IBufferElementData
    {
        /// <summary>
        /// A node entity.
        /// </summary>
        public Entity Node;

        /// <summary>
        /// Provides implicit conversion of an <see cref="Entity"/> to a Node element.
        /// </summary>
        /// <param name="e">The entity to convert</param>
        /// <returns>A new buffer element.</returns>
        public static implicit operator ConnectionGraph(Entity e)
        {
            return new ConnectionGraph {Node = e};
        }
    }

    public class FracturingSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        private struct CheckConnectivityMap : IJobForEachWithEntity_EB<ConnectionGraph>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public BufferFromEntity<Connection> ConnectionData;
            [ReadOnly] public ComponentDataFromEntity<StaticAnchor> AnchoredNodeData;

            public void Execute(Entity entity, int index, DynamicBuffer<ConnectionGraph> graph)
            {
                var count = 0;
                var depth = graph.Length;
                
                for (var i = 0; i < graph.Length; i++)
                {
                    var node = graph[i].Node;
                    if (!FindAnchorNode(node, index, EntityCommandBuffer, depth, ref count))
                    {
                        if (ConnectionData.Exists(node))
                            EntityCommandBuffer.RemoveComponent<Connection>(index, node);
                        Debug.Log(node + "is disconnected");
                        //Disconnect(node, index, EntityCommandBuffer);
                    }
                }

                // Destroy if no more connections exist
                if (graph.Length.Equals(0))
                    EntityCommandBuffer.DestroyEntity(index, entity);
            }

            private bool FindAnchorNode(Entity node, int index, EntityCommandBuffer.Concurrent EntityCommandBuffer, int depth, ref int count)
            {
                count++;
                if (count > 99)
                    return false;
                
                if (AnchoredNodeData.Exists(node))
                    return true;
                if (!ConnectionData.Exists(node))
                    return false;

                if (ConnectionData.Exists(node))
                {
                    for (var i = 0; i < ConnectionData[node].Length; i++)
                    {
                        return FindAnchorNode2(node, index, EntityCommandBuffer, depth, ref count);
                    }
                }

                return false;
            }

            private bool FindAnchorNode2(Entity node, int index, EntityCommandBuffer.Concurrent EntityCommandBuffer, int depth, ref int count)
            {
                if (AnchoredNodeData.Exists(node))
                    return true;
                if (!ConnectionData.Exists(node))
                    return false;

                
                if (ConnectionData.Exists(node))
                {
                    for (var i = 0; i < ConnectionData[node].Length; i++)
                    {
                        var subnode = ConnectionData[node][i].Node;
                        if (AnchoredNodeData.Exists(subnode))
                            return true;
                        if (!ConnectionData.Exists(subnode))
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
                
                if (!ConnectionData.Exists(node))
                    return;
                
                if (ConnectionData.Exists(node))
                {
                    EntityCommandBuffer.RemoveComponent<Connection>(index, node);

                    for (var i = 0; i < ConnectionData[node].Length; i++)
                    {
                        Disconnect(node, index, EntityCommandBuffer, depth, ref count);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var connectivityMapJob = new CheckConnectivityMap
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                ConnectionData = GetBufferFromEntity<Connection>(true),
                AnchoredNodeData = GetComponentDataFromEntity<StaticAnchor>(true)
            };
            var checkConnectivityHandle = connectivityMapJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(checkConnectivityHandle);

            return checkConnectivityHandle;
        }
    }
}