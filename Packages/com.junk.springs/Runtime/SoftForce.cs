using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Springs
{
    [InternalBufferCapacity(120)]
    public struct SoftForce : IBufferElementData
    {
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator float3 (SoftForce s) { return s.Force; }
        public static implicit operator SoftForce (float3 s) { return new SoftForce { Force = s }; }
    
        // Actual value each buffer element will store.
        public float3 Force;
    }
}