using Unity.Entities;
using Unity.Mathematics;

namespace thelebaron.bee
{
    public struct BallisticMotion : IComponentData
    {
        public float Drag; // 0.0025f
        public float GravityFalloff; // 0.4
    }
}