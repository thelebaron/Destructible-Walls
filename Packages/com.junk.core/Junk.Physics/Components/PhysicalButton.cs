using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    public struct PhysicalButton: IComponentData
    {
        public bool Use;

        public float3 Direction;
        public float3 InitialPosition;
        public float  PressDepth; // Depth the button should move when pressed
        public float  PressSpeed; // Speed of the press and release

        public float MaxMoveTime;
        public float InternalTimer;
    }
}