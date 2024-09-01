using Unity.Entities;
using UnityEngine;

namespace Junk.Transforms.Hybrid
{
    public class SinMoveAuthoring : MonoBehaviour
    {
        public float amplitude = 0.025f; // Height of the sine wave
        public float frequency = 1.0f; // Speed of the sine wave

        public class SinMoveAuthoringBaker : Baker<SinMoveAuthoring>
        {
            public override void Bake(SinMoveAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SinMove
                {
                    Amplitude = authoring.amplitude,
                    Frequency = authoring.frequency
                });
            }
        }
    }
}