using Unity.Entities;

namespace Junk.LineRenderer
{
    public struct LineFade : IComponentData
    {
        public float FadeDelay;
        public float FadeTimer;
        public float FadeSpeed;
    }
}