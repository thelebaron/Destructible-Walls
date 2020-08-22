using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// Component added to a damaging entity.
    /// </summary>
    public struct Damage : IComponentData
    {
        public int DamageAmount;
    }
    
    /// <summary>
    /// Added to a damage entity that contains a damage component. Tells other systems who the originating entity is.
    /// </summary>
    public struct Instigator : IComponentData
    {
        public Entity Value;
    }
}