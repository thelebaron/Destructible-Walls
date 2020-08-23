using Unity.Entities;
using Unity.Mathematics;

namespace thelebaron.bee
{
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }
}