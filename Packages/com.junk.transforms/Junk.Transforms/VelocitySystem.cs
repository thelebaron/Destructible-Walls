using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Transforms
{
    [BurstCompile]    
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct VelocitySystem : ISystem
    {
        private EntityQuery                          query;
        private EntityQuery                          fallOffQuery;
        private ComponentTypeHandle<Velocity>        velocityTypeHandle;
        private ComponentTypeHandle<VelocityFalloff> velocityFalloffTypeHandle;
        private ComponentTypeHandle<LocalTransform>  localTransformTypeHandle;

        [BurstCompile]
        private struct FalloffVelocity : IJobChunk
        {
            [ReadOnly] public float                                FixedDeltaTime;
            [ReadOnly] public float                                DeltaTime;
            public            ComponentTypeHandle<Velocity>        VelocityType;
            [ReadOnly] public ComponentTypeHandle<VelocityFalloff> VelocityFalloffType;
            public            uint                                 LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                const float gravity    = 9.807f;
                var         oneVector  = new float3(1f, 1f, 1f);
                var         velocities = chunk.GetNativeArray(ref VelocityType);
                var         falloffs   = chunk.GetNativeArray(ref VelocityFalloffType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var velocity    = velocities[i];
                    var falloff     = falloffs[i];

                    // Gravity is factored into the velocity, can be ignored by setting the gravityfactor to 0
                    velocity.Value.y -= gravity * FixedDeltaTime * falloff.GravityFalloff; // was using fixedDeltaTime

                    // Also apply a small amount of drag over time
                    /*if (math.length(velocity.Value) > 0)
                    {
                        velocity.Value -= falloff.Drag * FixedDeltaTime * maths.one;
                    }*/
                    // above but using math.select
                    velocity.Value = math.select(velocity.Value, velocity.Value - falloff.Drag * FixedDeltaTime *    oneVector, math.length(velocity.Value) > 0);
                    
                    velocities[i]   = velocity;
                }
            } 
        }
        
        [BurstCompile]
        private struct VelocityMove : IJobChunk
        {
            [ReadOnly] public float                               FixedDeltaTime;
            [ReadOnly] public float                               DeltaTime;
            public            ComponentTypeHandle<LocalTransform> LocalTransformType;
            public            ComponentTypeHandle<Velocity>       VelocityType;
            public            uint                                LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                bool changed         = chunk.DidOrderChange(LastSystemVersion) || chunk.DidChange(ref VelocityType, LastSystemVersion);
                var  localTransforms = chunk.GetNativeArray(ref LocalTransformType);
                var  velocities      = chunk.GetNativeArray(ref VelocityType);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var localTransform = localTransforms[i];
                    var velocity        = velocities[i];

                    // We align to velocity so that the trajectory changes with distance. Can be disabled.
                    localTransform.Rotation = quaternion.LookRotation(math.normalize(velocity.Value), math.up());

                    // Finally move the entity
                    localTransform.Position += velocity.Value * FixedDeltaTime;

                    localTransforms[i] = localTransform;
                    velocities[i]      = velocity;
                }
            }
        }

        [BurstCompile]
        public void OnCreate(ref  SystemState state)
        {
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<Velocity>()
                .WithAllRW<LocalTransform>()
                .Build(ref state);
            fallOffQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<VelocityFalloff>()
                .WithAllRW<Velocity>()
                .Build(ref state);

            velocityTypeHandle        = state.GetComponentTypeHandle<Velocity>();
            velocityFalloffTypeHandle = state.GetComponentTypeHandle<VelocityFalloff>(true);
            localTransformTypeHandle  = state.GetComponentTypeHandle<LocalTransform>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            velocityTypeHandle.Update(ref state);
            velocityFalloffTypeHandle.Update(ref state);
            localTransformTypeHandle.Update(ref state);
            
            state.Dependency = new FalloffVelocity
            {
                FixedDeltaTime      = SystemAPI.Time.fixedDeltaTime,
                DeltaTime           = SystemAPI.Time.DeltaTime,
                VelocityType        = velocityTypeHandle,
                VelocityFalloffType = velocityFalloffTypeHandle,
                
            }.Schedule(fallOffQuery, state.Dependency);
            
            state.Dependency = new VelocityMove
            {
                FixedDeltaTime           = SystemAPI.Time.fixedDeltaTime,
                DeltaTime                = SystemAPI.Time.DeltaTime,
                LocalTransformType = localTransformTypeHandle,
                VelocityType             = velocityTypeHandle,
                LastSystemVersion        = state.LastSystemVersion
            }.Schedule(query, state.Dependency);
        }
    }
}