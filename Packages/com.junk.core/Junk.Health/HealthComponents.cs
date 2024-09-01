using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Health
{
    /// <summary>
    /// Basic component that stores limited information about health.
    /// </summary>
    [Serializable]
    public struct HealthData : IComponentData, IEnableableComponent
    {
        // a vector representing the current value, a maximum value, and a last frame damage value.
        public float3 Value;
        
        public float Current => Value.x;
        public float Maximum => Value.y;
        public float LastDamage => Value.z;
        public float Percentage => math.clamp(Current / Maximum, 0f, 1f);
        
        /// <summary>
        /// Adds a value to the current health.
        /// </summary>
        /// <param name="value">Value to add to the current health</param>
        /// <param name="clampMaximum">If true clamps the maximum value to the current maximum health</param>
        public void AddHealth(float value, bool clampMaximum)
        {
            var health = Value;
            health.x = math.clamp(health.x + value, 0, health.y); // maths.select(health.x
            Value    = health;
            //Value.x = math.max(Value.x + health, Maximum);
        }
        
        /// <summary>
        /// Subtracts a value from the current value, with the option to clamp the minimum value to zero.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="clampToZero">If true, clamps the current value to zero. If false, allows for negative values.</param>
        public void TakeDamage(float damage, bool clampToZero)
        {
            Value.z = damage;
            var minimum = clampToZero ? 0 : Value.x - damage;
            Value.x = math.max(minimum, Value.x - damage);
        }

        public static HealthData GetDefault()
        {
            return new HealthData
            {
                Value = new float3(100f, 100f, 0f)
            };
        }

        public void ClearLastDamage()
        {
            Value.z = 0;
        }

        public int GetCurrentHealthToInt()
        {
            return (int)Value.x;
        }
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
    
    /// <summary>
    /// Stores multiple damage events into a buffer, to allow for multiple
    /// sources of damage per frame.
    /// </summary>
    [Serializable]
    [InternalBufferCapacity(0)]
    public struct HealthDamageBuffer : IBufferElementData
    {
        // Actual value each buffer element will store.
        public DamageData Value;
        
        public static implicit operator DamageData(HealthDamageBuffer e)
        {
            return e.Value;
        }

        public static implicit operator HealthDamageBuffer(DamageData e)
        {
            return new HealthDamageBuffer {Value = e};
        }
    }

    /// <summary>
    /// Tag component that when exists on an entity, uses the destroy pipeline to auto destroy the entity when it has reached zero health.
    /// Don't use if you want more control over what happens what happens to the entity before it is destroyed.
    /// </summary>
    public struct SimpleDestroy : IComponentData {}

    /*
    /// <summary>
    /// Tracks the per frame changes to a health component, if it is added to an entity.
    /// </summary>
    [Serializable]
    public struct HealthState : IComponentData
    {
        //public bool   Invulnerable;
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


