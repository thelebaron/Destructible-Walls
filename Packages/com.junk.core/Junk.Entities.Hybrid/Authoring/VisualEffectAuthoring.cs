using Unity.Entities;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    [RequireComponent(typeof(VFXController))]
    public class VisualEffectAuthoring : MonoBehaviour
    {
        public class VisualEffectAuthoringBaker : Baker<VisualEffectAuthoring>
        {
            public override void Bake(VisualEffectAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new VisualEffectComponentData());
                AddComponentObject(entity, authoring.GetComponent<VFXController>());
            }
        }
    }

    public struct VisualEffectComponentData : IComponentData
    {
    }
}