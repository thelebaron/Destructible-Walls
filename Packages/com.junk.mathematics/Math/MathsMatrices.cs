using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Math
{
    public  static partial class maths
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetScale(this float4x4 matrix)
        {
            return math.length(matrix.c0.xyz);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScale3(this float4x4 matrix)
        {
            return new float3(math.length(matrix.c0.xyz), math.length(matrix.c1.xyz), math.length(matrix.c2.xyz));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetScale(this LocalToWorld ltw)
        {
            return ltw.Value.GetScale();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetScale3(this LocalToWorld ltw)
        {
            return ltw.Value.GetScale3();
        }
        
        
        // Extension method to expand the extents of an AABB
        public static void Expand(this ref AABB aabb, float3 expansion)
        {
            aabb.Center  -= expansion * 0.5f;
            aabb.Extents += math.abs(expansion) * 0.5f;
        }
    }
}