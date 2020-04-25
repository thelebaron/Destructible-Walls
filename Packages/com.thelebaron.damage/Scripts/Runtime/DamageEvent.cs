using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// A damage event is a component that gets created with its own entity so that multiple instances 
    /// may be handled in a single frame by a damage stack.
    /// Useage: 
    /// Create new entity, 
    /// Create DamageEvent component 
    /// Attach to created entity.
    /// </summary>
    public struct DamageEvent: IComponentData
    {
        public int Amount;
        public Entity Receiver;
        public Entity Sender;
    }
}