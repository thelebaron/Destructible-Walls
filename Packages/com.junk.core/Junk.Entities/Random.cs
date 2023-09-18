using System;
using System.Runtime.CompilerServices;
using Junk.Math;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Junk.Entities
{
    /// <summary>
    /// For generating random values for an emitter
    /// </summary>
    /// <remarks>
    /// <see cref="Unity.Mathematics.Random"/> maintains internal state so this component must be set on the emitter entity any time it is used
    /// </remarks>
    [Serializable]
    public struct Rng : IComponentData
    {
        public Random Value;
    }
    
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct RngBakerSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var rng in SystemAPI.Query<RefRW<Rng>>())
            {
                rng.ValueRW.Value = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, uint.MaxValue));
            }
        }
    }
    
    public static class RandomExtensions
    {
        
        public static float Probability(this ref Rng rng, float min, float max)
        {
            return rng.Value.NextFloat(min, max);
        }
        
        public static float Random01(this ref Rng rng)
        {
            return rng.Value.NextFloat(0.0f, 1.0f);
        }

        public static float RandomRange(this ref Rng rand, Junk.Math.Range range)
        {
            return rand.Value.NextFloat(range.Start, range.End);
        }
        
        public static float Range(this ref Rng rng, float a, float b)
        {
            return rng.Value.NextFloat(a, b);
        }
        /// <summary>
        /// see https://forum.unity.com/threads/random-insideunitsphere-circle.920045/#post-6023861
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 InsideSphere(this ref Rng rand)
        {
            var phi   = rand.Value.NextFloat(2 * math.PI);
            var theta = math.acos(rand.Value.NextFloat(-1f, 1f));
            var r     = math.pow(rand.Value.NextFloat(), 1f / 3f);
            var x     = math.sin(theta) * math.cos(phi);
            var y     = math.sin(theta) * math.sin(phi);
            var z     = math.cos(theta);
            return r * new float3(x, y, z);
        }
    }
}