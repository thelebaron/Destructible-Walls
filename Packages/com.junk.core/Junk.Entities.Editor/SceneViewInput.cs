using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace Junk.Entities.Editor
{
    //[InitializeOnLoad]
    public class SceneViewInput : EditorWindow
    {
        private static SceneViewInput window         = null;
        Action<SceneView>             onSceneGUIFunc = null;
        public static SceneViewInput  Instance;

        [MenuItem("Window/SceneViewInput")]
        private static void Init()
        {
            EditorWindow.GetWindow<SceneViewInput>().Show();
            window                   =  EditorWindow.GetWindow<SceneViewInput>();
            window.onSceneGUIFunc    =  OnSceneGUI;
            SceneView.duringSceneGui += window.onSceneGUIFunc;
        }

        /*static SceneViewInput()
        {
            Init();
        }*/

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= onSceneGUIFunc;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.keyCode == KeyCode.F)
            {
                Debug.Log("Focus selected in scene view");
                if (World.DefaultGameObjectInjectionWorld == null)
                    return;
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                var    view = SceneView.lastActiveSceneView;
                float3 pos  = default;

                var selectedEntity  = EntitySelection.Entity;
                var hasLocalToWorld = entityManager.HasComponent<LocalToWorld>(selectedEntity);
                Debug.Log(" selected " + selectedEntity + " " + hasLocalToWorld + " " + entityManager.HasComponent<LocalToWorld>(selectedEntity));

                if (hasLocalToWorld)
                    pos = entityManager.GetComponentData<LocalToWorld>(selectedEntity).Position;

                view.pivot = hasLocalToWorld ? entityManager.GetComponentData<LocalToWorld>(selectedEntity).Position : view.pivot;
                //view.cameraDistance = 10;
                // Set the camera distance to 5 units from the target object
                view.camera.transform.position = sceneView.pivot + sceneView.camera.transform.rotation * Vector3.forward * 5;
            }
        }
    }
}