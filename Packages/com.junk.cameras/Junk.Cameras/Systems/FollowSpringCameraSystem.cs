using Junk.Springs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Entities.SystemAPI;

namespace Junk.Cameras
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct FollowSpringCameraSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (follow, transform, entity) in  Query<RefRW<FollowSpring>,RefRW<LocalTransform>>().WithEntityAccess())
            {
                var posSpring = GetComponent<Spring>(follow.ValueRW.PositionSpring);
                var rotSpring = GetComponent<Spring>(follow.ValueRW.RotationSpring);

                var myLocalToWorld = GetComponent<LocalToWorld>(entity);
                var localToWorld = GetComponent<LocalToWorld>(follow.ValueRW.Target);


                var dir = localToWorld.Position - myLocalToWorld.Position;
                
                
                posSpring.RestState = localToWorld.Position;
                
                SetComponent(follow.ValueRW.PositionSpring, posSpring);
                SetComponent(follow.ValueRW.RotationSpring, rotSpring);
                
                var localTransform = GetComponent<LocalTransform>(entity);
                localTransform.Position = posSpring.Value;
                localTransform.Rotation = math.slerp(localTransform.Rotation, localToWorld.Rotation, 0.1f * deltaTime * 30f);
                SetComponent(entity, localTransform);
                
                //transform.ValueRW.Position = posSpring.Value;
                //transform.ValueRW.Rotation = rotSpring.Value;
            }
        }
    }
}