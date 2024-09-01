using Unity.Entities;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    #if UNITY_EDITOR
    [DisallowMultipleComponent]
    public class DestructibleAuthoring : MonoBehaviour
    {
        public FractureCache      Cache;
        public GameObject         Prefab;
    }
    
    public class DestructibleBaker : Baker<DestructibleAuthoring>
    {
        public override void Bake(DestructibleAuthoring authoring)
        {
            // Do not bake if no fracture cache is exists
            if(authoring.Cache == null || authoring.Prefab == null)
                return;
            
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Breakable
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Data = authoring.Cache.BakeToBlob(this)
            });
            
            SetComponentEnabled<Breakable>(entity, true);
        }
    }
    #endif
}