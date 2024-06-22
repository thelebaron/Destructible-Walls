using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Cameras
{
    /// <summary>
    /// Cant remember but I think this should find a scene camera and attach it to the player
    /// Creates a hybrid camera entity.
    /// Adds transform components to the camera entity.
    /// Adds managed camera transform components to the camera entity.
    /// Parents hybrid camera entity to player camera entity
    /// Removes CameraAnchor component from player camera entity
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CameraAnchorSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // So technically singletons could be used but this should work fine
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<CameraAnchor>().WithEntityAccess())
            {
                foreach (var (lt, mainCameraData, mainCameraEntity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MainCameraData>>().WithNone<Parent>().WithEntityAccess())
                {
                    mainCameraData.ValueRW.NearClipPlane = 0.01f;
                    if (SystemAPI.HasBuffer<Child>(entity))
                    {
                        var cameraChildBuffer = state.EntityManager.GetBuffer<Child>(entity);
                        cameraChildBuffer.Add(new Child {Value = mainCameraEntity});
                        //Debug.Log("cameraChildBuffer.Add(new Child {Value = mainCameraEntity});");
                    }
                    else
                    {
                        var cameraChildBuffer = ecb.AddBuffer<Child>(entity);
                        //ecb.AppendToBuffer();
                        //Debug.Log("Do I need to use appendtobuffer");
                        cameraChildBuffer.Add(new Child {Value = mainCameraEntity});
                        //Debug.Log("cameraChildBuffer.Add(new Child {Value = mainCameraEntity});");
                    }
            
                    ecb.AddComponent(mainCameraEntity, new Parent{Value = entity});
                    ecb.SetComponent(mainCameraEntity, LocalTransform.Identity);
                }
                ecb.RemoveComponent<CameraAnchor>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

        }
    }
}