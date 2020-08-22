using System;
using Unity.Entities;

namespace thelebaron.damage
{
    
    /// <summary>
    /// A health on a parented or separate entity, which sends its value back to the root health.
    /// The multiplier adjusts this damage value. For ragdoll/character setups.
    /// </summary>
    public struct HealthLink : IComponentData
    {
        // The main entity with a health component.
        public Entity Value;
        // A value to modify incoming damage instances by.
        public float Multiplier;
    }
    
    /// <summary>
    /// A health on a parented or separate entity, which sends its value back to the root health.
    /// </summary>
    public struct CompositeHealth : IComponentData
    {
        public int Value;
    }

    /// <summary>
    /// Basic component that stores limited information about health.
    /// </summary>
    public struct Health : IComponentData
    {
        public int Value;
        public int MaxValue;
        
        public int LastDamageValue;
        public Entity LastDamagerEntity;
    }

    // This describes the number of buffer elements that should be reserved
    // in chunk data for each instance of a buffer. In this case, 8 integers
    // will be reserved (32 bytes) along with the size of the buffer header
    // (currently 16 bytes on 64-bit targets)
    // 8 elements = 20 bytes * 8 = 160 bytes
    /// <summary>
    /// Stores multiple damage events into a buffer, to allow for multiple
    /// sources of damage per frame.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct HealthFrameBuffer : IBufferElementData
    {
        // Actual value each buffer element will store.
        public DamageInstance Value;
        
        public static implicit operator DamageInstance(HealthFrameBuffer e)
        {
            return e.Value;
        }

        public static implicit operator HealthFrameBuffer(DamageInstance e)
        {
            return new HealthFrameBuffer {Value = e};
        }
    }
    
    /// <summary>
    /// Tracks the per frame changes to a health component, if it is added to an entity.
    /// </summary>
    public struct HealthState : IComponentData
    {
        public bool   Invulnerable;
        public int    LastDamageValue;
        public Entity LastDamagerEntity;
        public float  TimeLastHurt;
        public int    DamageTaken;
    }
    
    /// <summary>
    /// Tag to exclude health from taking negative damage/exclude from queries on a certain threshold.
    /// Never added automatically(by this package) to allow for custom behaviour; ie
    /// Entity reaches sub zero health, your code checks for this. Add custom death logic, then add tag
    /// and entity is no longer processed by the health system.
    /// </summary>
    public struct Dead : IComponentData
    {
        
    }
    
    
    /*
     *
     * 
    HealthSystem

    Health // Value - 4 bytes
    HealthState // MaxValue, LastDamageValue, LastDamagerEntity, TimeLastHurt - 20 bytes
    HealthBuffer // contains DamageEvents - 0 bytes
    HealthHistory // DamageInfo buffer

    Damage
    Instigator

    LocalHealth
    ChildHealth
    ParentHealth
    Health
    CompositeHealth
     */
    //[Serializable]
    //[WriteGroup(typeof(LocalToWorld))]
   // [WriteGroup(typeof(LocalToParent))]
   // public struct Translation : IComponentData
    //{
    //    public float3 Value;
   // }
}


