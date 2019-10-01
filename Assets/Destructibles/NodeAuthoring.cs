using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Destructibles
{
    
    
    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public List<Transform> Connections = new List<Transform>();
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //dstManager.AddComponentData(entity, new Translation());
            //Add the node buffer
            var nodebuffer = dstManager.AddBuffer<Node>(entity);
            for (int i = 0; i < Connections.Count; i++)
            {
                var otherentity = conversionSystem.GetPrimaryEntity(Connections[i].gameObject);
                
                nodebuffer.Add(otherentity);
            }

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
            }

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
    /// A node contains all other entities connected to the current node.
    /// </summary>
    public struct Node : IBufferElementData
    {
        /// <summary>
        /// A node entity.
        /// </summary>
        public Entity Value;
        
        /// <summary>
        /// Provides implicit conversion of an <see cref="Entity"/> to a Node element.
        /// </summary>
        /// <param name="e">The entity to convert</param>
        /// <returns>A new buffer element.</returns>
        public static implicit operator Node(Entity e)
        {
            return new Node {Value = e};
        }
    }
}