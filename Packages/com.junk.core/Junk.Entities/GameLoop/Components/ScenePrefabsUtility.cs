#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Junk.Entities
{
    public static class ScenePrefabsUtility
    {
        [EditorOnly]
        public static void GrabScenePrefabs()
        {
            var gameObjects  = Object.FindObjectsOfType<GameObject>();
            var scenePrefabs = Object.FindObjectOfType<ScenePrefabs>();
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