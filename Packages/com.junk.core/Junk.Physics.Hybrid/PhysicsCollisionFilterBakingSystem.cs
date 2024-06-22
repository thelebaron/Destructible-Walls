using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace Junk.Physics.Hybrid
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct PhysicsCollisionFilterBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (collisionFilter, physicsCollider) in SystemAPI.Query<PhysicsCollisionFilterBakingData, PhysicsCollider>())
            {
                physicsCollider.Value.Value.SetCollisionFilter(collisionFilter.CollisionFilter);
            }
        }
    }
}