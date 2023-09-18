using Junk.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Cameras
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct BillboardSystem : ISystem
    {
        private EntityQuery                            billboardQuery;
        private EntityQuery                            cameraQuery;
        private ComponentTypeHandle<MainCamera>        mainCameraType;
        private ComponentTypeHandle<LocalToWorld>      localToWorldType;
        private ComponentTypeHandle<LocalTransform>    localTransformType;
        private ComponentTypeHandle<Billboard>         billboardType;
        private ComponentTypeHandle<BillboardVelocity> velocityType;
        private ComponentTypeHandle<BillboardSimple>   simpleType;

        [BurstCompile]
        private struct CameraLocalToWorld : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld>     LocalToWorldType;
            public            NativeArray<LocalToWorld>             LocalToWorld;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var cameraLocalToWorld = chunk.GetNativeArray<LocalToWorld>(ref LocalToWorldType)[0];
                LocalToWorld[0] = cameraLocalToWorld;
            }
        }
        
        [BurstCompile]
        private struct BillboardJob : IJobChunk
        {
            [ReadOnly] public                                      NativeArray<LocalToWorld>              CameraLocalToWorld;
            public                                                 ComponentTypeHandle<LocalTransform>    LocalTransformType;
            [NativeDisableParallelForRestriction][ReadOnly] public ComponentTypeHandle<LocalToWorld>      LocalToWorldType;
            [ReadOnly]                                      public ComponentTypeHandle<Billboard>         BillboardType;
            [ReadOnly]                                      public ComponentTypeHandle<BillboardSimple>   BillboardSimpleType;
            [ReadOnly]                                      public ComponentTypeHandle<BillboardVelocity> BillboardVelocityType;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var localTransforms           = chunk.GetNativeArray(ref LocalTransformType);
                var localToWorlds             = chunk.GetNativeArray(ref LocalToWorldType);
                var cameraPosition            = CameraLocalToWorld[0].Position;
                var cameraRotation            = CameraLocalToWorld[0].Rotation;
                var chunkBillboards           = chunk.GetNativeArray(ref BillboardType);
                var chunkHasSimpleBillboard   = chunk.Has(ref BillboardSimpleType);
                var chunkHasVelocityBillboard = chunk.Has(ref BillboardVelocityType);
                
                if (chunkHasVelocityBillboard)
                {
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var billboard    = chunkBillboards[i];
                        var localToWorld = localToWorlds[i];
                        var rotation     = localTransforms[i];
                        var direction    = cameraPosition - localToWorld.Position;
                    
                        //var x = maths.select()
                        var axis = maths.up; //default
                        axis = math.select(axis, maths.right, billboard.Axis.x);
                        axis = math.select(axis, maths.up, billboard.Axis.y);
                        axis = math.select(axis, maths.forward, billboard.Axis.z);
                        
                        var forward = math.normalizesafe(maths.projectOnPlane(direction, axis)); // Y AXIS, use right/forward for xz 
                        axis = forward;//math.normalizesafe(math.forward(Camera.rot));
                        // We align Z to velocity
                        var velRotation = quaternion.LookRotation(math.normalizesafe(math.forward(rotation.Rotation)), axis);
                        rotation.Rotation    = math.mul(velRotation, quaternion.EulerXYZ(90,0,0));
                        localTransforms[i] = rotation;
                    }
                }
                else if (chunkHasSimpleBillboard)
                {
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var rotation     = localTransforms[i];
                        rotation.Rotation    = maths.RotateTowards(rotation.Rotation, cameraRotation, 130);
                        localTransforms[i] = rotation;
                    }
                }
                else
                {
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var billboard    = chunkBillboards[i];
                        var localToWorld = localToWorlds[i];
                        var rotation     = localTransforms[i];
                        var direction    = cameraPosition - localToWorld.Position;
                        var dot          = math.dot(direction, maths.up);
                        var orthogonal   = dot.Equals(1f) || dot.Equals(-1f);
                    
                        //var x = maths.select()
                        var axis = maths.up; //default
                        axis = math.select(axis, maths.right, billboard.Axis.x);
                        axis = math.select(axis, maths.up, billboard.Axis.y);
                        axis = math.select(axis, maths.forward, billboard.Axis.z);

                        var forward = math.normalizesafe(maths.projectOnPlane(direction, axis)); // Y AXIS, use right/forward for xz 
                        rotation.Rotation = orthogonal ? localTransforms[i].Rotation : quaternion.LookRotationSafe(forward, maths.up);

                        localTransforms[i] = rotation;
                    }
                }
                /*
                if (chunk.Has(BillboardXType))
                {
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var      localToWorld = localToWorlds[i];
                        var      direction  = cameraPosition - localToWorld.Position;
                        var      dot        = math.dot(direction, maths.up);
                        var      orthogonal = dot == 1f || dot == -1f;

                        //var x = maths.select()
                        var forward = math.normalizesafe(maths.projectOnPlane(direction, maths.right));
                        var rotation = new Rotation { Value = orthogonal ? chunkRotations[i].Value : quaternion.LookRotationSafe(forward, maths.up)};
                        
                        chunkRotations[i] = rotation;
                    }
                }
                else if (chunk.Has(BillboardYType))
                {
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        var localToWorld = localToWorlds[i];
                        var direction    = cameraPosition - localToWorld.Position;
                        var dot          = math.dot(direction, maths.up);
                        var orthogonal   = dot == 1f || dot == -1f;
                        
                        var forward = math.normalizesafe(maths.projectOnPlane(direction, maths.up));      // Y AXIS, use right/forward for xz                  
                        var rotation = new Rotation {
                            Value = orthogonal ? chunkRotations[i].Value : quaternion.LookRotationSafe(forward, maths.up)
                        };
                        
                        chunkRotations[i] = rotation;
                    }
                }
                else if (chunk.Has(BillboardZType))
                {
                    for (int i = 0; i < chunk.Count; i++)
                    {
                        var localToWorld = localToWorlds[i];
                        var direction    = cameraPosition - localToWorld.Position;
                        var dot          = math.dot(direction, maths.up);
                        var orthogonal   = dot == 1f || dot == -1f;
                        var forward      = math.normalizesafe(maths.projectOnPlane(direction, maths.forward));                        
                        var rotation = new Rotation { Value = orthogonal ? chunkRotations[i].Value : quaternion.LookRotationSafe(forward, maths.up) };
                        
                        chunkRotations[i] = rotation;
                    }
                }*/
            }
        }
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCamera>();

            var builderCamera = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<MainCamera>();
            cameraQuery = state.GetEntityQuery(builderCamera);

            var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Billboard>()
            .WithAllRW<LocalTransform>()
            .WithAllRW<LocalToWorld>()
            .WithAny<BillboardVelocity,BillboardSimple>();
            billboardQuery   = state.GetEntityQuery(builder);
            
            mainCameraType   = state.GetComponentTypeHandle<MainCamera>(true);
            localToWorldType = state.GetComponentTypeHandle<LocalToWorld>(true);
            localTransformType     = state.GetComponentTypeHandle<LocalTransform>();
            billboardType    = state.GetComponentTypeHandle<Billboard>(true);
            velocityType     = state.GetComponentTypeHandle<BillboardVelocity>(true);
            simpleType       = state.GetComponentTypeHandle<BillboardSimple>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var array            = state.WorldUnmanaged.UpdateAllocator.AllocateNativeArray<LocalToWorld>(1);
            
            mainCameraType.Update(ref state);
            localToWorldType.Update(ref state);
            localTransformType.Update(ref state);
            billboardType.Update(ref state);
            velocityType.Update(ref state);
            simpleType.Update(ref state);

            state.Dependency = new CameraLocalToWorld
            {
                LocalToWorldType      = localToWorldType,
                LocalToWorld = array
            }.Schedule(cameraQuery, state.Dependency);
            
            state.Dependency = new BillboardJob
            {
                CameraLocalToWorld    = array,//cameraLocalToWorld,
                LocalToWorldType      = localToWorldType,
                LocalTransformType          = localTransformType,
                BillboardType         = billboardType,
                BillboardVelocityType = velocityType,
                BillboardSimpleType   = simpleType
            }.Schedule(billboardQuery, state.Dependency);
        }


    }
}