using System.Collections.Generic;
using System.Linq;
using thelebaron.Damage;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{
    public struct FractureNode : IComponentData {}
    
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
            dstManager.AddComponentData(entity, new Health{ Value = 10, Max = 10 });
            dstManager.AddComponentData(entity, new FractureNode());
            
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

        public RigidTransform worldFromA => new RigidTransform(gameObject.transform.rotation, gameObject.transform.position);
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

        [ExcludeComponent(typeof(ConnectionGraph))]
        [RequireComponentTag(typeof(ConnectionGraph))]
        private struct ConnectivityMap : IJobForEachWithEntity<Translation>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public BufferFromEntity<Connection> JointsFromEntity;
            public void Execute(Entity entity, int index, ref Translation translation)
            {
                var graph = EntityCommandBuffer.AddBuffer<ConnectionGraph>(index, entity);

                var connectionJoints = JointsFromEntity[entity];
                
                
                for (var i = 0; i < connectionJoints.Length; i++)
                {
                    graph.Add(connectionJoints[i].Node);

                    if (JointsFromEntity.Exists(connectionJoints[i].Node))
                    {
                        var distantJoints = JointsFromEntity[connectionJoints[i].Node];
                        for (var j = 0; j < distantJoints.Length; j++)
                        {
                            
                        }
                    }
                    

                    
                }
            }

        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return inputDeps;
        }
    }
}