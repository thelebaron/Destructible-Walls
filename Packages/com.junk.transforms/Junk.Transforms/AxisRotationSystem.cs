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
    public partial struct AxisRotationSystem : ISystem
    {
        private EntityQuery                               query;
        private ComponentTypeHandle<AxisRotation>         axisRotationTypeHandle;
        private ComponentTypeHandle<PostRotationEulerXYZ> postRotationEulerXYZTypeHandle;

        [BurstCompile]
        private struct AxisRotationJob : IJobChunk
        {
            public            float                                     DeltaTime;
            [ReadOnly] public ComponentTypeHandle<AxisRotation>         AxisRotationTypeHandle;
            public ComponentTypeHandle<PostRotationEulerXYZ> PostRotationEulerXYZTypeHandle;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var chunkAxisRotation         = chunk.GetNativeArray(ref AxisRotationTypeHandle);
                var chunkPostRotationEulerXYZ = chunk.GetNativeArray(ref PostRotationEulerXYZTypeHandle);
                var count                     = chunk.Count;
                
                for (int i = 0; i < count; i++)
                {
                    var axisRotation = chunkAxisRotation[i];
                    var postRotationEulerXYZ = chunkPostRotationEulerXYZ[i];
                    
                    var x   = DeltaTime * axisRotation.SpeedX;
                    var y   = DeltaTime * axisRotation.SpeedY;
                    var z   = DeltaTime * axisRotation.SpeedZ;
                    var xyz = new float3(x,y,z);
                
                    postRotationEulerXYZ.Value += xyz;
                    chunkPostRotationEulerXYZ[i] = postRotationEulerXYZ;
                }
            }
        }
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PostRotationEulerXYZ, AxisRotation>()
                .Build(ref state);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new AxisRotationJob
            {
                DeltaTime                      = SystemAPI.Time.DeltaTime,
                AxisRotationTypeHandle         = SystemAPI.GetComponentTypeHandle<AxisRotation>(true),
                PostRotationEulerXYZTypeHandle = SystemAPI.GetComponentTypeHandle<PostRotationEulerXYZ>()
            }.Schedule(query, state.Dependency);
        }
    }
}