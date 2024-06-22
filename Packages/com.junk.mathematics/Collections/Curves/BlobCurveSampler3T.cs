﻿using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Collections
{
    public struct BlobCurveSampler3<T> : IBlobCurveSampler<T>
        where T : unmanaged
    {
        public readonly BlobAssetReference<BlobCurve3> Curve;
        private         BlobCurveCache                 cache;

        public BlobCurveSampler3(BlobAssetReference<BlobCurve3> curve)
        {
            Check.Assume(UnsafeUtility.SizeOf<T>() == UnsafeUtility.SizeOf<float3>());

            this.Curve = curve;
            this.cache = BlobCurveCache.Empty;
        }

        public bool IsCreated => this.Curve.IsCreated;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Evaluate(in float time)
        {
            var r = this.Curve.Value.Evaluate(time, ref this.cache);
            return UnsafeUtility.As<float3, T>(ref r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T EvaluateIgnoreWrapMode(in float time)
        {
            var r = this.Curve.Value.EvaluateIgnoreWrapMode(time, ref this.cache);
            return UnsafeUtility.As<float3, T>(ref r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T EvaluateWithoutCache(in float time)
        {
            var r = this.Curve.Value.Evaluate(time);
            return UnsafeUtility.As<float3, T>(ref r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T EvaluateIgnoreWrapModeWithoutCache(in float time)
        {
            var r = this.Curve.Value.EvaluateIgnoreWrapMode(time);
            return UnsafeUtility.As<float3, T>(ref r);
        }
    }
}