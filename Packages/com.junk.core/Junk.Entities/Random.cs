using System;
using System.Runtime.CompilerServices;
using Junk.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Junk.Entities
{
    /// <summary>
    /// For generating random values per entity.
    /// Note this should never be disabled, a disabled state is used to indicate a seed has not been set
    /// </summary>
    /// <remarks>
    /// <see cref="Unity.Mathematics.Random"/> maintains internal state so this component must be set on the emitter entity any time it is used
    /// </remarks>
    [Serializable]
    public struct Rng : IComponentData, IEnableableComponent
    {
        public Random Value;
        
        public static implicit operator Random (Rng rng)
        {
            return rng.Value;
        }
        
        public static implicit operator Rng (Random rng)
        {
            return new Rng {Value = rng};
        }
    }
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct RngSeedSystem : ISystem
    {
        private EntityQuery seedQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            seedQuery    = new EntityQueryBuilder(Allocator.Temp).WithDisabled<Rng>().WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities).Build(ref state);
        }

        [BurstCompile]
        private struct ChunkSeedJob : IJobChunk
        {
            public uint                     SystemVersion;
            public ComponentTypeHandle<Rng> RngTypeHandle;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var seed = SystemVersion + (uint)unfilteredChunkIndex;
                var rngs = chunk.GetNativeArray(ref RngTypeHandle);
                
                var e = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (e.NextEntityIndex(out var entityIndex))
                {
                    seed += (uint)entityIndex;
                    rngs[entityIndex] = new Rng {Value = Random.CreateFromIndex(seed)};
                    chunk.SetComponentEnabled<Rng>(ref RngTypeHandle, entityIndex, true);
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var version = state.LastSystemVersion;
            
            state.Dependency = new ChunkSeedJob
            {
                SystemVersion = version,
                RngTypeHandle = SystemAPI.GetComponentTypeHandle<Rng>(),
            }.Schedule(seedQuery, state.Dependency);
        }
    }

    
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct RngBakerSystem : ISystem
    {
        private Random systemRandom;
        public void OnCreate(ref SystemState state)
        {
            systemRandom = Random.CreateFromIndex((uint)UnityEngine.Random.Range(0, uint.MaxValue));
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            
            foreach (var rng in SystemAPI.Query<RefRW<Rng>>().WithOptions(
                         EntityQueryOptions.IncludePrefab | 
                         EntityQueryOptions.IgnoreComponentEnabledState | 
                         EntityQueryOptions.IncludeDisabledEntities))
            {
                rng.ValueRW.Value = Random.CreateFromIndex(systemRandom.NextUInt());
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