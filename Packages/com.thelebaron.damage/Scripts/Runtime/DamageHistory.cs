using Unity.Entities;
using Unity.Mathematics;

namespace thelebaron.damage
{
    [InternalBufferCapacity(4)]
    public struct DamageHistory : IBufferElementData
    {
        public float TimeOccured;
        public float3 DamageLocation;
        public bool TookDamage;
        public int Damage;
        public Entity Instigator;
        public Entity Target;
        public DamageInstance LastDamageInstance;
    
    }


}