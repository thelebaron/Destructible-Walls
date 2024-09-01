using Unity.Entities;

namespace Junk.Transforms
{
    public struct SinMove : IComponentData
    {
        public float Amplitude ; // Height of the sine wave
        public float Frequency;  // Speed of the sine wave
    }
}