using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// Basic component that stores limited information about health.
    /// </summary>
    [GenerateAuthoringComponent]
    public struct Health : IComponentData
    {
        public int  Value;
        public int  Max;
        public int  DamageTaken; // todo move to separate component
        public Entity Damager; // todo move to separate component
        
        public void ApplyDamage(DamageEvent damageEvent)
        {
            if (Value <= 0)
                return;
            
            Value -= damageEvent.Amount;
            DamageTaken = damageEvent.Amount;
            Damager = damageEvent.Sender;
        }

        public void ApplyHealth(int amount)
        {
            if (amount <= 0)
                return;
            
            Value += amount;
        }
    }
    
}