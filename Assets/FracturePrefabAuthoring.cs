using Unity.Entities;
using UnityEngine;

public class FracturePrefabAuthoring : MonoBehaviour
{
    public GameObject FracturePrefab;

    public class FracturePrefabAuthoringBaker : Baker<FracturePrefabAuthoring>
    {
        public override void Bake(FracturePrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FracturePrefabComponentData
            {
                Prefab = GetEntity(authoring.FracturePrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct FracturePrefabComponentData : IComponentData
{
    public Entity Prefab;
}

