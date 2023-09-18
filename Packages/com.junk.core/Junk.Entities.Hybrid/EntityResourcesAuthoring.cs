using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    public class EntityResourcesAuthoring : MonoBehaviour
    {
        public GameObject DebrisPebble;
        public GameObject DecalBloodSplatter;
        public GameObject BloodSpraySpawnerLarge;
    }

    public class EntityResourcesBaker : Baker<EntityResourcesAuthoring>
    {
        public override void Bake(EntityResourcesAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var data = new EntityResourcesSingleton
            {
                DebrisPebble           = GetEntity(authoring.DebrisPebble, TransformUsageFlags.Dynamic),
                DecalBloodSplatter     = GetEntity(authoring.DecalBloodSplatter, TransformUsageFlags.Renderable),
                BloodSpraySpawnerLarge = GetEntity(authoring.BloodSpraySpawnerLarge, TransformUsageFlags.Dynamic)
            };

            AddComponent(entity, data);
        }
    }
}