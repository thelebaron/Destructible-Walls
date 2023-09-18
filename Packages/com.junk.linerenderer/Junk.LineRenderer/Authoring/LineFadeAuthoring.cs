using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Junk.LineRenderer
{
    public class LineFadeAuthoring : MonoBehaviour
    {
        public float fadeDelay = 2f;
        public float fadeSpeed = 5f;
    }
    
    public class LineFadeBaker : Baker<LineFadeAuthoring>
    {
        public override void Bake(LineFadeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LineFade
            {
                FadeDelay = authoring.fadeDelay,
                FadeSpeed = authoring.fadeSpeed
            });
            AddComponent(entity, new URPMaterialPropertyBaseColor{Value = 1});

        }
    }
}