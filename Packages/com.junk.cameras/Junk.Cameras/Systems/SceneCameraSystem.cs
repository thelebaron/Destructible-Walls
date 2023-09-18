#if UNITY_EDITOR
//[WorldSystemFilter(WorldSystemFilterFlags.Editor)]
using Junk.Entities;
using Junk.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Cameras
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(MainCameraSystem))]
    public partial class SceneCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<UnmanagedKeyboard>();
            RequireForUpdate<UnmanagedMouse>();
        }
        
        protected override void OnUpdate()
        {
            var keyboard = SystemAPI.GetSingleton<UnmanagedKeyboard>();
            var mouse    = SystemAPI.GetSingleton<UnmanagedMouse>();
            
            var up                   = keyboard.spaceKey.isPressed;
            var down                 = keyboard.leftCtrlKey.isPressed;
            var leftButtonIsPressed  = mouse.leftButton.isPressed;
            var rightButtonIsPressed = mouse.rightButton.isPressed;

            /* conflict with regular game
             if (leftButtonIsPressed || rightButtonIsPressed)
                Cursor.visible = false;
            else
                Cursor.visible = true;*/
            
            var leftButtonReleasedThisFrame  = mouse.leftButton.wasReleasedThisFrame;
            var rightButtonReleasedThisFrame = mouse.rightButton.wasReleasedThisFrame;
            
            var mousePos        = (float2) mouse.position;
            var mouseDelta      = mouse.delta;
            var mouseWheelDelta = mouse.scroll;
            
            var newDelta = mouseDelta;
            // Account for scaling applied directly in Windows code by old input system.
            newDelta *= 0.5f;
            // Account for sensitivity setting on old Mouse X and Y axes.
            newDelta *= 0.1f;
            newDelta *= -1;


            foreach (var (localTransform, mainCamera, sceneCamera, rotationEulerXYZ, localToWorld, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MainCamera>, RefRW<SceneCamera>, RefRW<RotationEulerXYZ>, RefRO<LocalToWorld>>().WithAll<CameraReference>().WithEntityAccess())
            {
                var reference = EntityManager.GetComponentObject<CameraReference>(entity);
                if(reference.Camera == null)
                    return;
                
                localTransform.ValueRW.Position = reference.Camera.transform.position;
                localTransform.ValueRW.Rotation = reference.Camera.transform.rotation;
                
                mainCamera.ValueRW.NearClipPlane  = reference.Camera.nearClipPlane;
                mainCamera.ValueRW.Orthographic   = reference.Camera.orthographic;
                mainCamera.ValueRW.LocalToWorldRO = new LocalToWorld{Value = float4x4.TRS(localTransform.ValueRW.Position, localTransform.ValueRW.Rotation, Vector3.one)};
                
                            
                //var delta = math.normalizesafe(sceneCamera.Delta);
                sceneCamera.ValueRW.Delta            = mouseDelta / 3.5f;
                sceneCamera.ValueRW.DistanceToCamera = 10;
                
                var screen = new Rect(0, 0, Screen.width, Screen.height);

                //if (screen.Contains(UnityEngine.Input.mousePosition))
                //sceneCamera.PreviousMousePosition = Input.mousePosition;
                
                var y = 0f;
                if (up)
                    y += 1/10f;
                
                if (down)
                    y -= 1/10f;
                localTransform.ValueRW.Position.y += y;
                
                if (leftButtonIsPressed && !rightButtonIsPressed)
                {
                    UpdateYaw(ref sceneCamera.ValueRW, ref rotationEulerXYZ.ValueRW);
                    {
                        // depth pan
                        //cameraState = ViewTool.Pan;
                        var dir = math.normalizesafe(localToWorld.ValueRO.Forward) * sceneCamera.ValueRW.Delta.y / 15;
                        //fwd               += localToWorld.Right * sceneCamera.Delta.x;// / 5;
                        dir.y                           =  0;
                        localTransform.ValueRW.Position += dir;
                    }
                }
                
                if (rightButtonIsPressed)
                {
                    if (!leftButtonIsPressed)
                    {
                        UpdatePitch(ref sceneCamera.ValueRW, ref rotationEulerXYZ.ValueRW);
                        UpdateYaw(ref sceneCamera.ValueRW, ref rotationEulerXYZ.ValueRW);
                    }

                    if (leftButtonIsPressed)
                    {
                        // depth pan
                        //cameraState = ViewTool.Pan;
                        var dir = math.normalizesafe(localToWorld.ValueRO.Up) * sceneCamera.ValueRW.Delta.y / 7;
                        dir += localToWorld.ValueRO.Right * sceneCamera.ValueRW.Delta.x / 7;
                        //fwd.y             =  0;
                        localTransform.ValueRW.Position += dir;
                    }
                }
                
                {
                    //scroll translation
                    var fwd = math.normalizesafe(localToWorld.ValueRO.Forward) * math.clamp((float)mouseWheelDelta.y, -1, 1) / 2;
                    localTransform.ValueRW.Position += fwd;
                }
                
                //transform.position = localTransform.Position; 
                //transform.rotation = new quaternion(localToWorld.Value);
                sceneCamera.ValueRW.PreviousMousePosition = mousePos;
                
                // Reset deltas
                if (leftButtonReleasedThisFrame || rightButtonReleasedThisFrame)
                {
                    //sceneCamera.Yaw = 0;
                    //sceneCamera.Pitch = 0;
                }
            }
        }
        
        

        private static void UpdateYaw(ref SceneCamera sceneCamera, ref RotationEulerXYZ rotationEuler)
        {
            sceneCamera.Yaw       += sceneCamera.Delta.x/2;
            sceneCamera.Yaw       =  math.@select(sceneCamera.Yaw, 0, sceneCamera.Yaw < -360);
            sceneCamera.Yaw       =  math.@select(sceneCamera.Yaw, 0, sceneCamera.Yaw > 360);
            rotationEuler.Value.y =  math.radians(sceneCamera.Yaw);
        }
        
        private static void UpdatePitch(ref SceneCamera sceneCamera, ref RotationEulerXYZ rotationEuler)
        {
            sceneCamera.Pitch     += sceneCamera.Delta.y;
            sceneCamera.Pitch     =  math.@select(sceneCamera.Pitch, -180, sceneCamera.Pitch < -180);
            sceneCamera.Pitch     =  math.@select(sceneCamera.Pitch, 180, sceneCamera.Pitch > 180);
            rotationEuler.Value.x =  math.radians(-sceneCamera.Pitch);
        }

        /*
        float ScreenToWorldDistance(float screenDistance, float distanceFromCamera)
        {
            Vector3 start = camera.ScreenToWorldPoint(Vector3.forward * distanceFromCamera);
            Vector3 end   = camera.ScreenToWorldPoint( new Vector3(screenDistance, 0f, distanceFromCamera));
            return CopySign(Vector3.Distance(start, end), screenDistance);
        }

        /// <summary>
        /// Return the magnitude of X with the sign of Y.
        /// </summary>
        float CopySign(float x, float y)
        {
            if(x < 0f && y < 0f || x > 0f && y > 0f || x == 0f || y == 0f)
                return x;

            return -x;
        }*/
    }
}
#endif