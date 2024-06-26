using Unity.Entities;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class BreakableAuthoring : MonoBehaviour
    {
        public FractureCache      FractureCache;
        public GameObject         FracturedPrefab;
    }
    
    public class BreakableBaker : Baker<BreakableAuthoring>
    {
        public override void Bake(BreakableAuthoring authoring)
        {
            // Do not bake if no fracture cache is exists
            if(authoring.FractureCache == null || authoring.FracturedPrefab == null)
                return;
            
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Breakable
            {
                Prefab = GetEntity(authoring.FracturedPrefab, TransformUsageFlags.Dynamic),
                Data = authoring.FractureCache.BakeToBlob(this)
            });
            
            SetComponentEnabled<Breakable>(entity, false);
        }
    }
}