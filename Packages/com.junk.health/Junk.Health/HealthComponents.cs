using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Hitpoints
{
    /// <summary>
    /// Basic component that stores limited information about health.
    /// </summary>
    [Serializable]
    public struct HealthData : IComponentData
    {
        public float Value;
        public float MaximumValue;
        
        public static HealthData GetDefault()
        {
            return new HealthData
            {
                Value = 100f,
                MaximumValue = 100f
            };
        }
        
        public static implicit operator float(HealthData e)
        {
            return e.Value;
        }
        
        public static implicit operator HealthData(float e)
        {
            return new HealthData {Value = e};
        }
    }
    
    [Serializable]
    public struct HealthMaximum : IComponentData
    {
        public float Value;
    }
    
    // A value to modify incoming damage instances by.
    [Serializable]
    public struct HealthMultiplier : IComponentData
    {
        public float Value;
    }
    
    [Serializable]
    public struct HealthParent : IComponentData
    {
        public Entity Value;
    }
    
    [Serializable]
    public struct HealthFeedback : IComponentData
    {
        public float LastFrameDamage;
    }
    
    /// <summary>
    /// The deathknell physics force to apply to the entity when it dies.
    /// </summary>
    [Serializable]
    public struct HealthPhysicsDeath : IComponentData
    {
        public float  Force;
        public float3 Direction;
        public float3 Point;
    }
    
    /// <summary>
    /// Stores multiple damage events into a buffer, to allow for multiple
    /// sources of damage per frame.
    /// </summary>
    [Serializable]
    [InternalBufferCapacity(8)]
    public struct HealthDamageBuffer : IBufferElementData
    {
        // Actual value each buffer element will store.
        public DamageInstance Value;
        
        public static implicit operator DamageInstance(HealthDamageBuffer e)
        {
            return e.Value;
        }

        public static implicit operator HealthDamageBuffer(DamageInstance e)
        {
            return new HealthDamageBuffer {Value = e};
        }
    }
    
    /// <summary>
    /// Tracks the per frame changes to a health component, if it is added to an entity.
    /// </summary>
    [Serializable]
    public struct HealthState : IComponentData
    {
        public bool   Invulnerable;
        public float  LastDamageValue;
        public Entity LastDamagerEntity;
        public float  TimeLastHurt;
        public float  TotalDamage;
        public float  DamageTaken;
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


