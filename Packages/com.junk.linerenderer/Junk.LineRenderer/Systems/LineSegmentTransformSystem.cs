using Junk.Cameras;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Junk.LineRenderer
{
    [RequireMatchingQueriesForUpdate]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(TransformSystemGroup), OrderLast = true)]
    public partial struct LineSegmentTransformSystem : ISystem
    {
        private EntityQuery lineFadeQuery;
        
        [BurstCompile]
        private struct FadeLineJob : IJobChunk
        {
            public float                                             DeltaTime;
            public ComponentTypeHandle<LineFade>                     LineFadeTypeHandle;
            public ComponentTypeHandle<URPMaterialPropertyBaseColor> UrpMaterialPropertyBaseColorTypeHandle;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var baseColorArray = chunk.GetNativeArray(ref UrpMaterialPropertyBaseColorTypeHandle);
                var lineFadeArray  = chunk.GetNativeArray(ref LineFadeTypeHandle);
                
                for (int i = 0; i < baseColorArray.Length; i++)
                {
                    var baseColor = baseColorArray[i];
                    var lineFade  = lineFadeArray[i];
                    
                    lineFade.FadeTimer += DeltaTime;
                    
                    if(lineFade.FadeTimer > lineFade.FadeDelay)
                    {
                        baseColor.Value = math.lerp(baseColor.Value, float4.zero, DeltaTime * lineFade.FadeSpeed);
                    }
                    lineFadeArray[i] = lineFade;
                    
                    baseColorArray[i] = baseColor;
                }
            }
        }

        [BurstCompile]
        private partial struct CameraPerspectiveUpdateJob : IJobEntity
        {
            public Entity                        CameraEntity;
            public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            
            public void Execute(ref LocalTransform localTransform, ref PostTransformMatrix postTransform, in LineSegment segment)
            {
                var cameraPosition = LocalToWorldLookup[CameraEntity].Position;
                var lineVec = segment.to - segment.from;
                localTransform.Rotation = quaternion.LookRotation(math.normalize(lineVec), math.normalize(cameraPosition - segment.from));
                localTransform.Position = segment.from;
                postTransform.Value     = float4x4.Scale(new float3 { x = segment.lineWidth, y = 1f, z = math.length(lineVec) });
            }
        }

        [BurstCompile]
        private partial struct CameraOrthographicUpdateJob : IJobEntity
        {
            public Entity CameraEntity;
            public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            
            public void Execute(ref LocalToWorld localToWorld, in LineSegment segment)
            {
                var cameraRotation = LocalToWorldLookup[CameraEntity].Rotation;
                var lineVec        = segment.to - segment.from;
                var rot            = quaternion.LookRotation(math.normalize(lineVec), math.mul(cameraRotation, new float3 { z = -1 }));
                var pos            = segment.from;
                var scale          = new float3 { x = segment.lineWidth, y = 1f, z = math.length(lineVec) };
                localToWorld.Value = float4x4.TRS(pos, rot, scale);
            }
        }
        
        [BurstCompile]
        public void OnCreate(ref  SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAllRW<LineFade, URPMaterialPropertyBaseColor>();
            lineFadeQuery = state.GetEntityQuery(builder);
            state.RequireForUpdate<MainCamera>();
        }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
#if UNITY_EDITOR
            //camera = SceneView.lastActiveSceneView.camera;
#endif

            var mainCamera      = SystemAPI.GetSingletonEntity<MainCamera>();
            state.Dependency = new CameraPerspectiveUpdateJob
            {
                CameraEntity = mainCamera,
                LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(),
            }.ScheduleParallel(state.Dependency);
            
            /*state.Dependency = new CameraOrthographicUpdateJob
            {
                CameraEntity       = mainCamera,
                LocalToWorldLookup = state.GetComponentLookup<LocalToWorld>(),
            }.ScheduleParallel(state.Dependency);*/
            
            state.Dependency = new FadeLineJob
            {
                DeltaTime                              = SystemAPI.Time.DeltaTime,
                LineFadeTypeHandle                     = SystemAPI.GetComponentTypeHandle<LineFade>(),
                UrpMaterialPropertyBaseColorTypeHandle = SystemAPI.GetComponentTypeHandle<URPMaterialPropertyBaseColor>()
            }.ScheduleParallel(lineFadeQuery, state.Dependency);
            
        }
    }
}