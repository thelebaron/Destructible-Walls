using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// A damage event is a component that gets created with its own entity so that multiple damage instances 
    /// may be handled in a single frame by a health buffer.
    /// Useage: 
    /// Create new entity, 
    /// Create DamageEvent component 
    /// Attach to created entity.
    ///
    /// 20 bytes
    /// </summary>
    public struct DamageInstance: IComponentData
    {
        public int Value;
        public Entity Receiver;
        public Entity Sender;
    }
}