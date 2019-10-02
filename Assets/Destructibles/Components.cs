using Unity.Entities;

namespace Destructibles
{
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
    /// 
    /// </summary>
    public struct NodeParent : IComponentData
    {
        public Entity Value;
    }
    
    /// <summary>
    /// 
    /// </summary>
    public struct GraphChild : IBufferElementData
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
        public static implicit operator GraphChild(Entity e)
        {
            return new GraphChild {Node = e};
        }
    }

    /*
     * JOINT CODE
     *
                    public RigidTransform worldFromA =>
            new RigidTransform(gameObject.transform.rotation, gameObject.transform.position);

        public RigidTransform worldFromB(Transform tr)
        {
            return new RigidTransform(tr.rotation, tr.position);
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
     * 
     */
}