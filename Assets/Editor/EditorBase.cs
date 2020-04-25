
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace thelebaron.CustomEditor
{
    public class EditorBase : EditorWindow
    {
        protected static Camera SceneCamera { get; set; }
        protected static Vector3 SceneCameraHitPosition { get; set; }
        protected static Quaternion SceneCameraRotation { get; set; }
        protected static Vector3 SceneCameraPosition { get; set; }
        protected static RaycastHit SceneCameraRaycastHit { get; set; }
        protected static float HitDistance { get; set; }
        

        protected virtual void OnFocus()
        {
        
        }
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected static void SceneCameraRaycast()
        {
            SceneCamera = SceneView.lastActiveSceneView.camera;
            if (SceneCamera == null) 
                return;
        
            var tr = SceneCamera.transform;
            var dir = tr.forward.normalized;//SpreadDirection(spread.value, spread.value, Quaternion.LookRotation(sceneCamera.transform.forward.normalized, Vector3.up));
            dir = dir.normalized * 100;

            var pos = tr.position;
            Physics.Raycast(pos, dir, out var raycastHit, 100);
        
        
            SceneCameraHitPosition = raycastHit.point;
            SceneCameraRotation = tr.rotation;
            SceneCameraPosition = pos;
            SceneCameraRaycastHit = raycastHit;
            HitDistance = raycastHit.distance;
        }

        protected virtual void Update()
        {
            SceneCameraRaycast();
        }
    }
}

#endif