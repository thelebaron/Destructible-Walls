/*
 Obsolete but kept for reference as how to bootstrap prior to scene loading
 using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

namespace Junk.Cameras
{
#if UNITY_EDITOR
    public static class EditorCameraInitialization
    {
        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            //Debug.Log("Project loaded in Unity Editor");
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() 
        {
            // do something
            //Debug.Log("OnScriptsReloaded loaded in Unity Editor");
            
            var worlds = World.All;
            if(worlds.Count.Equals(0))
                return;
            
            if(Application.isPlaying)
                return;
            
            //var e = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            //World.DefaultGameObjectInjectionWorld.EntityManager.SetName(e, "#UNITY_EDITOR Editor SceneCamera Entity");
            
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            var entity = entityManager.CreateEntity();
            entityManager.AddComponent<MainCamera>(entity);
            //entityManager.AddComponent<CopyTransformToGameObject>(entity);
            entityManager.AddComponent<LocalToWorld>(entity);
            entityManager.AddComponent<LocalTransform>(entity);
            entityManager.SetName(entity, "SceneCamera");
            //entityManager.AddComponentObject(entity, sceneCamera.transform);
            //entityManager.AddComponentObject(entity, sceneCamera);
            
            var sceneCamera = SceneView.lastActiveSceneView.camera;
            entityManager.AddComponentObject(entity, new CameraReference
            {
                Camera = sceneCamera
            });
            
        }
    }
    
    [InitializeOnLoad]
    public class Startup {
        static Startup()
        {
            //Debug.Log("Up and running");
        }
    }
    
#endif    

}*/