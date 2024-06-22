using Unity.Physics;

namespace Junk.Physics
{
    public static class PhysicsGlobals
    {
        public static CollisionFilter StaticEnvironment()
        {
            var staticEnvironment = CollisionFilter.Default;
            staticEnvironment.CollidesWith = 1;
            return staticEnvironment;
        }
    }
}