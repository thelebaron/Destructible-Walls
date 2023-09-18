using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Transforms
{
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }
    
    public struct VelocityFalloff : IComponentData
    {
        public float  Drag; // 0.0025f
        public float  GravityFalloff; // 0.4
        public float3 Gravity; // -9.8f.
        public float  WindFalloff; // 0.4
        public float3 Wind; // 0.0f
        
    }
}