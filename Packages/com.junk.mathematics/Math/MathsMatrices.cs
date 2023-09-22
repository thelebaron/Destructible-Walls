﻿using System.Runtime.CompilerServices;
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
    }
}