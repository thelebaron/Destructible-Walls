using Unity.Entities;

namespace thelebaron.damage
{
    public struct Damage : IComponentData
    {
        public int DamageAmount;
    }
    
    public struct Instigator : IComponentData
    {
        public Entity Value;
    }
}