﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Junk.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BlobCurve : IBlobCurve<float>
    {
        internal BlobCurveHeader             header;
        private  BlobArray<BlobCurveSegment> Segments;
        public unsafe ref BlobCurveHeader Header => ref UnsafeUtility.AsRef<BlobCurveHeader>(UnsafeUtility.AddressOf(ref this.header));
        public unsafe ref BlobArray<float> Times => ref UnsafeUtility.AsRef<BlobArray<float>>(UnsafeUtility.AddressOf(ref this.header.Times));
        public BlobCurveHeader.WrapMode WrapModePrev => this.header.WrapModePrev;
        public BlobCurveHeader.WrapMode WrapModePost => this.header.WrapModePost;
        public int SegmentCount => this.header.SegmentCount;
        public float StartTime => this.header.StartTime;
        public float EndTime => this.header.EndTime;
        public float Duration => this.header.Duration;
        
        // For junk animation clip baking
        public static ref BlobCurve Create(ref BlobBuilder builder, ref BlobCurve data, AnimationCurve curve, Allocator allocator = Allocator.Persistent)
        {
            InputCurveCheck(curve);
            var     keys               = curve.keys;
            var     keyFrameCount      = keys.Length;
            var     hasOnlyOneKeyframe = keyFrameCount == 1;
            var     segmentCount       = math.select(keyFrameCount - 1, 1, hasOnlyOneKeyframe);
            
            data.header.SegmentCount = segmentCount;
            data.header.WrapModePrev = BlobShared.ConvertWrapMode(curve.preWrapMode);
            data.header.WrapModePost = BlobShared.ConvertWrapMode(curve.postWrapMode);

            if (hasOnlyOneKeyframe)
            {
                var key0                                                   = keys[0];
                var timeBuilder                                            = builder.Allocate(ref data.header.Times, 4);
                timeBuilder[0]                            = timeBuilder[1] = timeBuilder[2] = timeBuilder[3] = key0.time;
                builder.Allocate(ref data.Segments, 1)[0] = new BlobCurveSegment(key0, key0);
            }
            else
            {
                var timeBuilder = builder.Allocate(ref data.header.Times, keyFrameCount + 2);
                var segBuilder  = builder.Allocate(ref data.Segments, segmentCount);
                for (int i = 0, j = 1; i < segmentCount; i = j++)
                {
                    var keyI = keys[i];
                    timeBuilder[j] = keyI.time;
                    segBuilder[i]  = new BlobCurveSegment(keyI, keys[j]);
                }

                data.header.StartTime          = keys[0].time;
                data.header.EndTime            = timeBuilder[keyFrameCount] = keys[segmentCount].time;
                timeBuilder[0]                 = float.MaxValue;
                timeBuilder[keyFrameCount + 1] = float.MinValue;
            }

            return ref data;
        }
        
        // For junk animation clip baking
        public static BlobAssetReference<BlobCurve> Create(AnimationCurve curve, Allocator allocator = Allocator.Persistent)
        {
            InputCurveCheck(curve);
            var     keys               = curve.keys;
            var     keyFrameCount      = keys.Length;
            var     hasOnlyOneKeyframe = keyFrameCount == 1;
            var     segmentCount       = math.select(keyFrameCount - 1, 1, hasOnlyOneKeyframe);
            var     builder            = new BlobBuilder(Allocator.Temp);
            ref var data               = ref builder.ConstructRoot<BlobCurve>();
            data.header.SegmentCount = segmentCount;
            data.header.WrapModePrev = BlobShared.ConvertWrapMode(curve.preWrapMode);
            data.header.WrapModePost = BlobShared.ConvertWrapMode(curve.postWrapMode);

            if (hasOnlyOneKeyframe)
            {
                var key0                                                   = keys[0];
                var timeBuilder                                            = builder.Allocate(ref data.header.Times, 4);
                timeBuilder[0]                            = timeBuilder[1] = timeBuilder[2] = timeBuilder[3] = key0.time;
                builder.Allocate(ref data.Segments, 1)[0] = new BlobCurveSegment(key0, key0);
            }
            else
            {
                var timeBuilder = builder.Allocate(ref data.header.Times, keyFrameCount + 2);
                var segBuilder  = builder.Allocate(ref data.Segments, segmentCount);
                for (int i = 0, j = 1; i < segmentCount; i = j++)
                {
                    var keyI = keys[i];
                    timeBuilder[j] = keyI.time;
                    segBuilder[i]  = new BlobCurveSegment(keyI, keys[j]);
                }

                data.header.StartTime          = keys[0].time;
                data.header.EndTime            = timeBuilder[keyFrameCount] = keys[segmentCount].time;
                timeBuilder[0]                 = float.MaxValue;
                timeBuilder[keyFrameCount + 1] = float.MinValue;
            }

            return builder.CreateBlobAssetReference<BlobCurve>(allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EvaluateIgnoreWrapMode(in float time, [NoAlias] ref BlobCurveCache cache)
        {
            var i = this.header.SearchIgnoreWrapMode(time, ref cache, out var t);
            return this.Segments[i].Sample(BlobShared.PowerSerial(t));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EvaluateIgnoreWrapMode(in float time)
        {
            var i = this.header.SearchIgnoreWrapMode(time, out var t);
            return this.Segments[i].Sample(BlobShared.PowerSerial(t));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Evaluate(in float time, [NoAlias] ref BlobCurveCache cache)
        {
            var i = this.header.Search(time, ref cache, out var t);
            return this.Segments[i].Sample(BlobShared.PowerSerial(t));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Evaluate(in float time)
        {
            var i = this.header.Search(time, out var t);
            return this.Segments[i].Sample(BlobShared.PowerSerial(t));
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void InputCurveCheck(AnimationCurve curve)
        {
            if (curve == null)
            {
                throw new NullReferenceException("Input curve is null");
            }

            if (curve.length == 0)
            {
                throw new ArgumentException("Input curve is empty (no keyframe)");
            }

            var keys = curve.keys;
            for (int i = 0, len = keys.Length; i < len; i++)
            {
                var k = keys[i];
                if (k.weightedMode != WeightedMode.None)
                {
                    Debug.LogWarning($"Weight Not Supported! Key[{i}, Weight[{k.weightedMode}, In{k.inWeight}, Out{k.outWeight}], Time{k.time}, Value{k.value}]");
                }
            }
        }
    }
}