using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    public class CanBleedAuthoring : MonoBehaviour
    {
        public class CanBleedBaker : Baker<CanBleedAuthoring>
        {
            public override void Bake(CanBleedAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Bleeds());
            }
        }
    }
}