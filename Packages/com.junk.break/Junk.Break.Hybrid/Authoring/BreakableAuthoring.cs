using Unity.Entities;
using UnityEngine;

namespace Junk.Break.Hybrid
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class BreakableAuthoring : MonoBehaviour
    {
        public FractureCache FractureCache;
        public GameObject    FracturedObject;
    }
    
    public class BreakableBaker : Baker<BreakableAuthoring>
    {
        public override void Bake(BreakableAuthoring authoring)
        {
            // Do not bake if no fracture cache is exists
            if(authoring.FractureCache == null || authoring.FracturedObject == null)
                return;
            
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Breakable
            {
                Prefab = GetEntity(authoring.FracturedObject, TransformUsageFlags.Dynamic)
            });
            
            SetComponentEnabled<Breakable>(entity, false);
        }
    }
}