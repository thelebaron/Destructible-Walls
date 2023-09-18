using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Hitpoints
{
    /// <summary>
    /// Stores damage history in a buffer. 
    /// </summary>
    [InternalBufferCapacity(4)]
    public struct DamageHistory : IBufferElementData
    {
        public float TimeOccured;
        public float3 DamageLocation;
        public bool TookDamage;
        public float Damage;
        public Entity Instigator;
        public Entity Target;
        public DamageInstance LastDamageInstance;
    
    }


}