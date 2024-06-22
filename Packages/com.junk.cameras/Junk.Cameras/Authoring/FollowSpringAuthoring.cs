using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static Unity.Entities.SystemAPI;
using static Junk.Springs.SpringBlobUtility;

namespace Junk.Cameras
{
    public class FollowSpringAuthoring : MonoBehaviour
    {
        public float RotationSpringStiffness = 1;
        public float RotationSpringDamping   = 0.1f;
        public float PositionSpringStiffness = 1;
        public float PositionSpringDamping   = 0.1f;
        public class SpringCameraFollowAuthoringBaker : Baker<FollowSpringAuthoring>
        {
            public override void Bake(FollowSpringAuthoring authoring)
            {
                var entity       = GetEntity(TransformUsageFlags.Dynamic);
                var followEntity = CreateAdditionalEntity(TransformUsageFlags.Dynamic, false, "Follow Camera Anchor");
                
                var positionSpring = this.CreateSpring(authoring.gameObject.name + " Position Spring", authoring.PositionSpringStiffness, authoring.PositionSpringDamping);
                var rotationSpring  = this.CreateSpring(authoring.gameObject.name + " Rotation Spring", authoring.RotationSpringStiffness, authoring.RotationSpringDamping);
                
                AddComponent(entity, new FollowSpring
                {
                    Target         = followEntity,
                    Distance       = 0,
                    PositionSpring = positionSpring,
                    RotationSpring = rotationSpring
                });
            }
        }
    }

    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct FollowSpringBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (follow, entity) in  Query<RefRW<FollowSpring>>().WithEntityAccess())
            {
                if (HasComponent<Parent>(entity))
                {
                    var parent = GetComponent<Parent>(entity);
                    //follow.ValueRW.Target = parent.Value;
                
                    var localToWorld  = GetComponent<LocalToWorld>(entity);
                    var worldPosition = localToWorld.Position;
                    var worldRotation = localToWorld.Rotation;
                    
                    var localPosition = GetComponent<LocalTransform>(entity).Position;
                    var localRotation = GetComponent<LocalTransform>(entity).Rotation;
                    
                    // switcheroo
                    ecb.RemoveComponent<Parent>(entity);
                    ecb.SetComponent<LocalTransform>(entity, LocalTransform.FromPositionRotation(worldPosition, worldRotation));
                    ecb.AddComponent<Parent>(follow.ValueRW.Target, new Parent { Value = parent.Value });
                    ecb.SetComponent<LocalTransform>(follow.ValueRW.Target, LocalTransform.FromPositionRotation(localPosition, localRotation));
                    
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}