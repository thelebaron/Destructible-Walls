using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Entities
{
    public class LightAuthoring : MonoBehaviour
    {
        public UnmanagedLightData Data = UnmanagedLightData.GetDefault();
        
        public class LightAuthoringBaker : Baker<LightAuthoring>
        {
            public override void Bake(LightAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, authoring.Data);
                AddComponent(entity, new UnmanagedLightReference());
                SetComponentEnabled<UnmanagedLightReference>(entity, false);
                
                /*var go = new GameObject("Light Entity");
                var light = go.AddComponent<Light>();
                light.intensity = authoring.Intensity;
                light.color = authoring.Color;
                light.enabled = authoring.Enabled;
                light.shadows = LightShadows.Soft;
                AddComponentObject(entity, light);*/
            }
        }
    }
}