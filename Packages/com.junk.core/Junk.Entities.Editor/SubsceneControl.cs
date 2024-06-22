using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Scenes;
using Unity.Scenes.Editor;

namespace Junk.Entities.Editor
{
    public static class SubsceneControl
    {
        //Menu item for sync all scenes in the project
        [MenuItem("Subscenes/Close all")]
        public static void CloseAllScenes()
        {
            var allSceneObjects = UnityEngine.Object.FindObjectsByType<SubScene>(FindObjectsSortMode.None);

            foreach (var sceneObject in allSceneObjects)
            {
                //Debug.Log(sceneObject.name);
                var sceneAsset = sceneObject.SceneAsset;

                if (sceneAsset != null)
                {
                    Scene scene = sceneObject.EditingScene;
                    if (scene.isLoaded && scene.isSubScene)
                        EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        //Menu item for sync all scenes in the project
        [MenuItem("Subscenes/Open all")]
        public static void OpenAllSubscenes()
        {
            var allSceneObjects = UnityEngine.Object.FindObjectsByType<SubScene>(FindObjectsSortMode.None);
            foreach (var subScene in allSceneObjects)
            {
                //Debug.Log(subScene.name);
                var sceneAsset = subScene.SceneAsset;
                var scenePath  = subScene.EditableScenePath;

                if (sceneAsset != null)
                {
                    Scene scene = subScene.EditingScene;
                    if (!scene.isLoaded && scene.isSubScene)
                    {
                        SubSceneUtility.EditScene(subScene);
                    }
                }
            }
        }
    }
}