using UnityEditor;

namespace Junk.Entities
{
    /* disabled
    [Unity.Entities.UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class ScenePrefabDeclareReferencedConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ScenePrefabs prefabs) =>
            {
                foreach (var prefab in prefabs.PrefabList)
                {
                    DeclareReferencedPrefab(prefab);
                }
            }).WithoutBurst().Run();
        }
    }
    public class ScenePrefabConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ScenePrefabs scenePrefabs) =>
            {
                DstEntityManager.AddBuffer<ScenePrefabData>(GetPrimaryEntity(scenePrefabs));
                foreach (var prefab in scenePrefabs.PrefabList)
                {
                    var p = GetPrimaryEntity(prefab);
                    var buffer = DstEntityManager.GetBuffer<ScenePrefabData>(GetPrimaryEntity(scenePrefabs));
                    buffer.Add(new ScenePrefabData { Prefab = p });
                }
            }).WithoutBurst().Run();
        }
    }*/
}