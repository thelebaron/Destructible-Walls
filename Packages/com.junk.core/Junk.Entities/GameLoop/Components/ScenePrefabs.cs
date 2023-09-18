using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    
    public class ScenePrefabs : MonoBehaviour
    {
        public List<GameObject> PrefabList = new List<GameObject>();
            
    }

    public class ScenePrefabsBaker : Baker<ScenePrefabs>
    {
        public override void Bake(ScenePrefabs authoring)
        {
            foreach (var prefab in authoring.PrefabList)
            {
                //DeclareReferencedPrefab(prefab);
                Debug.LogError("fix ScenePrefabsBaker");
                /*                foreach (var prefab in scenePrefabs.PrefabList)
                {
                    var p = GetPrimaryEntity(prefab);
                    var buffer = DstEntityManager.GetBuffer<ScenePrefabData>(GetPrimaryEntity(scenePrefabs));
                    buffer.Add(new ScenePrefabData { Prefab = p });
                }*/
            }
        }
    }
}