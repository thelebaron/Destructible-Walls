using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Health
{
    /// <summary>
    /// A "DamageInstance" is a component that gets created with its own entity so that multiple damage instances can both be created and processed in a single frame
    /// (though there is a 1 frame lag between creation to processing). 
    /// 
    /// Usage: 
    /// Create new entity, 
    /// Add DamageInstance component
    ///
    /// 20 bytes
    /// </summary>
    public struct DamageData : IComponentData
    {
        /// <summary>
        /// Amount of damage on this instance
        /// </summary>
        public float Amount;
        /// <summary>
        /// The entity that is receiving damage. 
        /// </summary>
        public Entity Receiver;
        /// <summary>
        /// The entity that the damage originates from.
        /// </summary>
        public Entity Sender;

        // Physics
        public float3 Point;
        public float3 Normal;

        public Entity          CreatedBy;
        public DamageEventData EventData;
    }
    
    public struct DamageEventData
    {
        public float3         Impulse;
        //public BodyIndexPair  BodyIndices;
        //public ConstraintType Type;
        public Entity         Entity;

        internal DamageData CreateDamageEvent()
        {
            return new DamageData
            {
                EventData = this
            };
        }
    }
}