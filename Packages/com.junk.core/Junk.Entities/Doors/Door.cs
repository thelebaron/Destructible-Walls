using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    public struct Door : IComponentData
    {
        public Entity    Trigger;
        public Entity    Collider;
        public float3    OpenPosition;
        public float3    ClosedPosition;
        public float     Speed;
        public bool      Use;
        public float     MovementDuraction;
        public DoorState State;
        public float     CurrentMoveTime;
    }
    
    public enum DoorState
    {
        Closed, Opened, Opening, Closing
    }

    public struct DoorTrigger : IComponentData
    {
        public Entity Door;
        public bool   Use;
    }

}