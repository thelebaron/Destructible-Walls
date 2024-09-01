using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Physics
{
    public struct Door : IComponentData
    {
        //public Entity              TriggerEntity;
        public Entity              ColliderEntity;
        public float3              OpenPosition;
        public float3              ClosedPosition;
        public float               Speed;
        
        public float3              Direction;
        
        public bool3               Axis;
        public bool                Use;
        public float               MovementDuration;
        public DoorState           State;
        public float               CurrentMoveTime;
        public FixedString128Bytes TargetName; // if this has a name, it is triggered by another entity.
        
        public DoorMovementType MovementType;
        public bool             NegativeDirection; // move in opposite direction
        public bool             RotationInOppositeOfTriggerer; // rotates in the opposite direction of the entity that triggered it, ie swing backwards if a player is in the way
    }
    
    public enum DoorState
    {
        Closed, Opened, Opening, Closing
    }

    public enum DoorMovementType
    {
        Linear,
        Rotational
    }
}