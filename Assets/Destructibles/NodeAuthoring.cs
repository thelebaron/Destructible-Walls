using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Math = Unity.Physics.Math;
using Unity.Physics.Extensions;

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

                var jointData = JointData.CreateFixed(transform.position, node.position, transform.rotation, node.rotation);
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