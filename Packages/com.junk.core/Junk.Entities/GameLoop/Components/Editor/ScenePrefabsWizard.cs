#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Junk.Entities
{
    [Obsolete]
    public class ScenePrefabsWizard : ScriptableWizard
    {
        public bool AllScenes = false;

        [MenuItem("Scene/Get All Scene Prefabs")]
        private static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<ScenePrefabsWizard>("Scene Prefabs Wizard", "Grab All Prefabs");
            // Clear progress bar
            EditorUtility.ClearProgressBar();
        }

        private void OnWizardUpdate()
        {
            helpString = "\nSelect an object, and bake guid\n";
        }

        private void OnWizardCreate()
        {
            GrabScenePrefabs();
        }

        private static void GrabScenePrefabs()
        {
            var gameObjects  = FindObjectsOfType<GameObject>();
            var scenePrefabs = FindObjectOfType<ScenePrefabs>();
            if(scenePrefabs == null)
            {
                var go = new GameObject("ScenePrefabs");
                scenePrefabs = go.AddComponent<ScenePrefabs>();
            }

            foreach (var gameObject in gameObjects)
            {
                if (gameObject.TryGetComponent<ScenePrefabAuthoring>(out var scenePrefabAuthoring))
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
                    {
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                        if(scenePrefabs.PrefabList.Contains(prefab))
                            continue;
                        scenePrefabs.PrefabList.Add(prefab);
                    }
                }
            }
        }
    }
}
#endif