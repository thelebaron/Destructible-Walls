using Unity.Entities;

namespace Destructibles
{
    /// <summary>
    /// Components for nodes
    /// </summary>
    public struct BreakableNode : IComponentData
    {
        //public float Strain;
        //public float MaxStrain;
    }

    public struct Strain : IComponentData
    {
        
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
    /// Component that gets attached to a chain entity
    /// </summary>
    public struct NodeAnchorBuffer : IBufferElementData
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
        public static implicit operator NodeAnchorBuffer(Entity e)
        {
            return new NodeAnchorBuffer {Node = e};
        }
    }
    
    /// <summary>
    /// Component that gets attached to a chain entity
    /// </summary>
    public struct NodeLinkBuffer : IBufferElementData
    {
        /// <summary>
        /// A node entity.
        /// </summary>
        public Entity Link;

        /// <summary>
        /// Provides implicit conversion of an <see cref="Entity"/> to a Node element.
        /// </summary>
        /// <param name="e">The entity to convert</param>
        /// <returns>A new buffer element.</returns>
        public static implicit operator NodeLinkBuffer(Entity e)
        {
            return new NodeLinkBuffer { Link = e };
        }
    }
    
    
    
    /// <summary>
    /// An anchor prevents a physicsvelocity from being added to an entity(unless its health meets the requirements). 
    /// </summary>
    public struct AnchorNode : IComponentData { }

    /// <summary>
    /// Tag that gets added to signify the node cannot be processed in a certain way.
    /// </summary>
    public struct BrokenNode : IComponentData { }

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
    
    
    
    

    
    ///  Node Links/Chain components  ///
    
    /// <summary>
    /// A list of entities that form a link to the anchored node.
    /// </summary>
    public struct GraphLink : IBufferElementData
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
        public static implicit operator GraphLink(Entity e)
        {
            return new GraphLink {Node = e};
        }
    }
    
    /// <summary>
    /// Component for the graph, a reference to the actual node.
    /// </summary>
    public struct GraphNode : IComponentData
    {
        public Entity Node;
    }
    
    /// <summary>
    /// Gets attached to an entity containing the above link, with an entity reference to the anchor.
    /// </summary>
    public struct GraphAnchor : IComponentData
    {
        public Entity Node;
        
        public static implicit operator GraphAnchor(Entity e)
        {
            return new GraphAnchor {Node = e};
        }
    }

    public struct DestroyLinkEvent : IComponentData
    {
        public Entity DestroyedLink;
    }
    
    /*
    public struct BreakEvent : IComponentData
    {
        public Entity NodeEntity;
        public Entity GraphEntity;
    }

        public struct Strain : IComponentData
    {
        //public float Radius;
        public float Current;
        public float Threshold;
    }
    
    
    
    
    
    
    
    
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