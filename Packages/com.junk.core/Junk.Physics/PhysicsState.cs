using Unity.Entities;
using Unity.Physics;
using Unity.Burst;

namespace Junk.Physics
{
    /// <summary>
    /// Data for switching between a kinematic to dynamic state and back
    /// </summary>
    public struct PhysicsState : IComponentData
    {
        public PhysicsMass     MassDynamic;
        public CollisionFilter FilterDynamic;
        public PhysicsMass     MassKinematic;
        public CollisionFilter FilterKinematic;

        public PhysicsState(PhysicsMass massKinematic, PhysicsMass massDynamic, 
            CollisionFilter kinematicFilter, CollisionFilter dynamicFilter)
        {
            MassDynamic = massDynamic;
            MassKinematic = massKinematic;
            FilterDynamic = dynamicFilter;
            FilterKinematic = kinematicFilter;
        }
    }
}
