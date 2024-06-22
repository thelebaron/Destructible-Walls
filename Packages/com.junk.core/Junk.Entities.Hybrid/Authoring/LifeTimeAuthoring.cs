using Unity.Entities;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    public class LifeTimeAuthoring : MonoBehaviour
    {
        [SerializeField] private float delay = 5f;
        
        public class LifeTimeBaker : Baker<LifeTimeAuthoring>
        {
            public override void Bake(LifeTimeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TimeDestroy { Value = authoring.delay });
            }
        }
    }
}

