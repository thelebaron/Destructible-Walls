using Unity.Entities;

namespace Junk.Hitpoints
{
    /// <summary>
    /// An abstract class to implement in order to create a system. This component is simply the value of damage when added to an entity that damages other entities. 
    /// </summary>
    /// <remarks>Implement a ComponentSystem subclass for systems that perform their work on the main thread or that
    /// use Jobs not specifically optimized for ECS. To use the ECS-specific Jobs, such as <see cref="HealthLink"/> or
    /// <see cref="HealthData"/>, implement <seealso cref="HealthSystem"/> instead.</remarks>
    public struct Damage : IComponentData
    {
        public float Value;
    }
    
    /// <summary>
    /// Added to an entity that contains a damage component. Tells other systems who the originating entity is.
    /// </summary>
    public struct Instigator : IComponentData
    {
        public Entity Value;
    }
}