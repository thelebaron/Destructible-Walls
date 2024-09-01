using Unity.Entities;

namespace Junk.Health
{
    /// <summary>
    /// This component is simply the value of damage when added to an entity that damages other entities. 
    /// </summary>
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