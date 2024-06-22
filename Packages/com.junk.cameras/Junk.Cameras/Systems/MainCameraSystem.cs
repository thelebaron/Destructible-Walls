using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Junk.Transforms.Hybrid;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Junk.Cameras
{
    /// <summary>
    /// 
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct MainCameraSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            /*
            if (!SystemAPI.HasSingleton<MainCamera>())
            {
                //Debug.Log("MainCamera Sys.");
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
            if (!SystemAPI.HasSingleton<MainCamera>())
                return;
            */

#if UNITY_EDITOR
            var isSceneView = !Application.isPlaying;
#endif
            foreach (var (mainCamera, localToWorld, hybridTransform, localTransform, entity) in SystemAPI.Query<RefRW<MainCameraData>, RefRO<LocalToWorld>, RefRO<HybridTransform>, RefRW<LocalTransform>>().WithAll<Camera>().WithEntityAccess())
            {
                var cameraPosition = float3.zero;
                var cameraRotation = quaternion.identity;
                
                // Managed camera copy settings
                var reference = state.EntityManager.GetComponentObject<Camera>(entity);
                if (reference == null)
                {
                    Debug.LogError("CameraReference is null");
                    return;
                }
                
#if UNITY_EDITOR
                if (isSceneView)
                {
                    //Debug.Log("isSceneView");
                    if (SceneView.lastActiveSceneView.camera != null)
                    {
                        
                        //Debug.Log("SceneView");
                        cameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
                        cameraRotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                        
                        //reference.Camera.transform.position = cameraPosition;
                        //reference.Camera.transform.rotation = cameraRotation;
                        
                        localTransform.ValueRW.Position = cameraPosition;
                        localTransform.ValueRW.Rotation = cameraRotation;
                        
                        mainCamera.ValueRW.NearClipPlane  = reference.nearClipPlane;
                        mainCamera.ValueRW.Orthographic   = reference.orthographic;
                        mainCamera.ValueRW.LocalToWorldRO = localToWorld.ValueRO;
                        return;
                    }
                    
                }
#endif

                if (hybridTransform.ValueRO.Options == TransformType.CopyToGameObject)
                {
                    reference.transform.position = localToWorld.ValueRO.Position;
                    reference.transform.rotation = localToWorld.ValueRO.Rotation;
                    return;
                }
                if (hybridTransform.ValueRO.Options == TransformType.CopyToEntity)
                {
                    localTransform.ValueRW.Position = reference.transform.position;
                    localTransform.ValueRW.Rotation = reference.transform.rotation;
                }
                
                mainCamera.ValueRW.NearClipPlane  = reference.nearClipPlane;
                mainCamera.ValueRW.Orthographic   = reference.orthographic;
                mainCamera.ValueRW.LocalToWorldRO = localToWorld.ValueRO;
                
                // Scene camera settings
            }
        }
    }
}