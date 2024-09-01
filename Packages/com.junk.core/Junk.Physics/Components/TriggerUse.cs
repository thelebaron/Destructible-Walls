using Unity.Entities;

namespace Junk.Entities
{
    public struct TriggerUse : IComponentData
    {
        public bool   Use;
    }
    
    public struct TriggerTouch : IComponentData
    {
        public bool   Touch;
    }
}