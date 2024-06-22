using Unity.Entities;

namespace Junk.Cameras
{
    public struct FollowSpring : IComponentData
    {
        public Entity Target;
        public float Distance;
        
        public Entity PositionSpring;
        public Entity RotationSpring;
    }
}