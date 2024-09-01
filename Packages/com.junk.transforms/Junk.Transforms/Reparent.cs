using Unity.Entities;

namespace Junk.Transforms
{
    /// <summary>
    /// Reparents the entity to be a child of the Target Entity.
    /// </summary>
    public struct Reparent : IComponentData
    {
        public Entity Target;
        public Entity Entity;
    }
    
    
}