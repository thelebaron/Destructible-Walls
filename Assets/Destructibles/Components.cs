using Unity.Entities;

namespace Destructibles
{
    /// <summary>
    /// Components for nodes
    /// </summary>
    public struct NodeBreakable : IComponentData
    {
        public Entity Value;
    }
    
    public struct Strain : IComponentData
    {
        //public float Radius;
        public float Current;
        public float Threshold;
    }
    
    
    /// <summary>
    /// A connection joint contains only the immediate entities which are connected to a node.
    /// </summary>
    public struct NodeNeighbor : IBufferElementData
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
        public static implicit operator NodeNeighbor(Entity e)
        {
            return new NodeNeighbor {Node = e};
        }
    }
    
    /// <summary>
    /// Component that gets attached to a chain entity.
    /// </summary>
    public struct Node : IComponentData
    {
        public Entity Value;
    }
    /// <summary>
    /// Component that gets attached to a chain entity
    /// </summary>
    public struct Anchor : IBufferElementData
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
        public static implicit operator Anchor(Entity e)
        {
            return new Anchor {Node = e};
        }
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    public struct NodeChild : IBufferElementData
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
        public static implicit operator NodeChild(Entity e)
        {
            return new NodeChild {Node = e};
        }
    }

    /// <summary>
    /// An static anchor prevents a physicsvelocity from being added to an entity. 
    /// </summary>
    public struct StaticAnchor : IComponentData
    {

    }
    /// <summary>
    /// An anchor prevents a physicsvelocity from being added to an entity. 
    /// </summary>
    public struct DynamicAnchor : IComponentData
    {

    }
    
    public struct Anchored : IComponentData
    {

    }
    public struct Unanchored : IComponentData
    {

    }

    
    
    /// <summary>
    /// A ConnectionGraph is created from the root/parent gameobject containing fracture parts.
    /// It is an array of all nodes within a certain fracturable object.
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
    
    
    
    
    /// <summary>
    /// 
    /// </summary>
    public struct Anchors : IBufferElementData
    {
        /// <summary>
        /// A node entity.
        /// </summary>
        public Entity NodeList;

        /// <summary>
        /// Provides implicit conversion of an <see cref="Entity"/> to a Node element.
        /// </summary>
        /// <param name="e">The entity to convert</param>
        /// <returns>A new buffer element.</returns>
        public static implicit operator Anchors(Entity e)
        {
            return new Anchors {NodeList = e};
        }
    }

    
    
    
    /// <summary>
    /// A list of entities that form a chain to the anchored node.
    /// </summary>
    public struct Chain : IBufferElementData
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
        public static implicit operator Chain(Entity e)
        {
            return new Chain {Node = e};
        }
    }
    
    
    
    public struct BreakEvent : IComponentData
    {
        public Entity NodeEntity;
        public Entity GraphEntity;
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