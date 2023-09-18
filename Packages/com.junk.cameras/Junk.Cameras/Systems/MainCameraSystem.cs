using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Junk.Transforms;
using Junk.Transforms.Hybrid;
using UnityEngine;

namespace Junk.Cameras
{
    /// <summary>
    /// 
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MainCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.HasSingleton<MainCamera>())
            {
                Camera mainCamera = null;
                // Note using Camera.main may not always work as it requires the camera to be tagged as MainCamera, so we use FindObjectOfType<Camera>()
                var cameras = Object.FindObjectsOfType(typeof(Camera));
                if (cameras == null || cameras.Length == 0)
                {
                    Debug.Log("No cameras found.");
                    return;
                }
                
                foreach (var camera in cameras)
                {
                    if (camera is Camera c && c.gameObject.CompareTag("MainCamera"))
                    {
                        mainCamera = c;
                        break;
                    }
                }
                
                if(mainCamera == null)
                {
                    Debug.Log("No cameras found.");
                    return;
                }
                
                var entity = EntityManager.CreateEntity();
                EntityManager.AddComponent<MainCamera>(entity);
                //EntityManager.AddComponentData<LocalToWorld>(entity, new LocalToWorld{Value = float4x4.TRS(Vector3.up * 10, quaternion.identity, Vector3.one)});
                EntityManager.AddComponentData<LocalToWorld>(entity, new LocalToWorld{Value =float4x4.identity});
                EntityManager.AddComponentData<LocalTransform>(entity, LocalTransform.FromScale(1)); // a scale other than 1 or zero will cause rotation errors
#if UNITY_EDITOR
                EntityManager.SetName(entity, "# MainCamera");
#endif
            
                EntityManager.AddComponentObject(entity, new CameraReference
                {
                    Camera = mainCamera
                });
                EntityManager.AddComponentObject(entity, new ManagedTransform
                {
                    Transform = mainCamera.transform
                });
                EntityManager.AddComponentData(entity, new HybridTransform
                {
                    Options = TransformType.CopyToGameObject
                });
                
                //mainCamera.name = "Converted Main Camera";
                
                EntityManager.AddComponent<RotationEulerXYZ>(entity);
                const float k_CursorIconSize = 64;
                EntityManager.AddComponentData(entity, new SceneCamera
                {
                    MouseCursorRect  = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize),
                    ScreenCenterRect = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize)
                });

                EntityManager.SetComponentEnabled<SceneCamera>(entity, false);
            }
            
            if(SystemAPI.HasSingleton<MainCamera>())
                return;
            
            foreach (var (mainCamera, localToWorld, hybridTransform, localTransform, entity) in SystemAPI.Query<RefRW<MainCamera>, RefRO<LocalToWorld>, RefRO<HybridTransform>, RefRW<LocalTransform>>().WithAll<CameraReference>().WithEntityAccess())
            {
                // Managed camera copy settings
                var reference = EntityManager.GetComponentObject<CameraReference>(entity);
                if (reference.Camera == null)
                {
                    Debug.LogError("CameraReference is null");
                    return;
                }

                if (hybridTransform.ValueRO.Options == TransformType.CopyToGameObject)
                {
                    reference.Camera.transform.position = localToWorld.ValueRO.Position;
                    reference.Camera.transform.rotation = localToWorld.ValueRO.Rotation;
                    return;
                }
                if (hybridTransform.ValueRO.Options == TransformType.CopyToEntity)
                {
                    localTransform.ValueRW.Position = reference.Camera.transform.position;
                    localTransform.ValueRW.Rotation = reference.Camera.transform.rotation;
                }
                
                mainCamera.ValueRW.NearClipPlane = reference.Camera.nearClipPlane;
                mainCamera.ValueRW.Orthographic = reference.Camera.orthographic;
                mainCamera.ValueRW.LocalToWorldRO = localToWorld.ValueRO;
                
                // Scene camera settings
            }
        }
    }
}