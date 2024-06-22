using System.Collections.Generic;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Junk.Entities.Hybrid
{
    /// <summary>
    /// Tag gameobject to be used as a container for scene objects(ie Scene Camera, VFX, Menu UI, etc)
    /// </summary>
    public class SceneObjects : MonoBehaviour
    {
        public static SceneObjects   Instance { get; private set; }
        public        List<SubScene> BakedLightingScenes = new List<SubScene>();

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                //Throw error if there is more than one instance of SceneObjects
                Debug.LogError("There is more than one instance of SceneObjects");
            }
        }

#if UNITY_EDITOR
        public static SceneObjects GetInstance()
        {
            if (Instance == null)
            {
                var sceneObjects = FindFirstObjectByType<SceneObjects>();
                if (sceneObjects != null)
                {
                    Instance = sceneObjects;
                }
                else
                {
                    throw new System.Exception("SceneObjects not found in scene!");
                }
            }

            return Instance;
        }

        // for use by TeamCityBuild.cs
        public void OpenBakingScenes()
        {
            var count = 0;
            foreach (var subScene in BakedLightingScenes)
            {
                var sceneAsset = subScene.SceneAsset;
                var scenePath  = subScene.EditableScenePath;

                if (sceneAsset != null)
                {
                    Scene scene = subScene.EditingScene;
                    if (!scene.isLoaded && scene.isSubScene)
                    {
                        Unity.Scenes.Editor.SubSceneUtility.EditScene(subScene);
                        count++;
                    }
                }
            }

            Debug.Log($"Opened {count} subscenes with baked lighting");
        }
        
        public bool BakingScenesOpen()
        {
            var areLoaded = false;
            foreach (var subScene in BakedLightingScenes)
            {
                var sceneAsset = subScene.SceneAsset;
                var scenePath  = subScene.EditableScenePath;

                if (sceneAsset != null)
                {
                    Scene scene = subScene.EditingScene;
                    if (scene.isLoaded && scene.isSubScene)
                    {
                        areLoaded = true;
                    }
                    else
                    {
                        areLoaded = false;
                        break;
                    }
                }
            }
            return areLoaded;
        }
#endif
    }
}