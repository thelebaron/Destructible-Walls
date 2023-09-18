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
    [UpdateBefore(typeof(LocalToWorldSystem))]
    public partial struct RotationEulerSystem : ISystem
    {
        
        private EntityQuery query;
        private EntityQuery rotationEulerQuery;
        private EntityQuery postRotationEulerXYZQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CompositeRotation>()
                .WithAllRW<LocalTransform>()
                //.WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build(ref state);
            
            rotationEulerQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<LocalTransform>()
                //.WithOptions(EntityQueryOptions.FilterWriteGroup)
                .Build(ref state);
            
            postRotationEulerXYZQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PostRotationEulerXYZ>()
                .WithAllRW<LocalTransform>()
                .Build(ref state);
        }
        
        /*[BurstCompile]
        struct ToCompositeRotation : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<PostRotation>             PostRotationType;
            //[ReadOnly] public ComponentTypeHandle<Rotation>                 RotationType;
            //[ReadOnly] public ComponentTypeHandle<RotationPivot>            RotationPivotType;
            //[ReadOnly] public ComponentTypeHandle<RotationPivotTranslation> RotationPivotTranslationType;
            public            ComponentTypeHandle<CompositeRotation>        CompositeRotationType;
            public            uint                                          LastSystemVersion;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var chunkRotationPivotTranslations = chunk.GetNativeArray(RotationPivotTranslationType);
                var chunkRotations = chunk.GetNativeArray(RotationType);
                var chunkPostRotation = chunk.GetNativeArray(PostRotationType);
                var chunkRotationPivots = chunk.GetNativeArray(RotationPivotType);
                var chunkCompositeRotations = chunk.GetNativeArray(CompositeRotationType);

                var hasRotationPivotTranslation = chunk.Has(RotationPivotTranslationType);
                var hasRotation = chunk.Has(RotationType);
                var hasPostRotation = chunk.Has(PostRotationType);
                var hasRotationPivot = chunk.Has(RotationPivotType);
                var count = chunk.Count;

                var hasAnyRotation = hasRotation || hasPostRotation;

                // 000 - Invalid. Must have at least one.
                // 001
                if (!hasAnyRotation && !hasRotationPivotTranslation && hasRotationPivot)
                {
                    var didChange = chunk.DidChange(RotationPivotType, LastSystemVersion);
                    if (!didChange)
                        return;

                    // Only pivot? Doesn't do anything.
                    for (int i = 0; i < count; i++)
                        chunkCompositeRotations[i] = new CompositeRotation {Value = float4x4.identity};
                }
                // 010
                else if (!hasAnyRotation && hasRotationPivotTranslation && !hasRotationPivot)
                {
                    var didChange = chunk.DidChange(RotationPivotTranslationType, LastSystemVersion);
                    if (!didChange)
                        return;

                    for (int i = 0; i < count; i++)
                    {
                        var translation = chunkRotationPivotTranslations[i].Value;

                        chunkCompositeRotations[i] = new CompositeRotation
                        {Value = float4x4.Translate(translation)};
                    }
                }
                // 011
                else if (!hasAnyRotation && hasRotationPivotTranslation && hasRotationPivot)
                {
                    var didChange = chunk.DidChange(RotationPivotTranslationType, LastSystemVersion);
                    if (!didChange)
                        return;

                    // Pivot without rotation doesn't affect anything. Only translation.
                    for (int i = 0; i < count; i++)
                    {
                        var translation = chunkRotationPivotTranslations[i].Value;

                        chunkCompositeRotations[i] = new CompositeRotation
                        {Value = float4x4.Translate(translation)};
                    }
                }
                // 100
                else if (hasAnyRotation && !hasRotationPivotTranslation && !hasRotationPivot)
                {
                    // 00 - Not valid
                    // 01
                    if (!hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(RotationType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var rotation = chunkRotations[i].Value;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = new float4x4(rotation, float3.zero)};
                        }
                    }
                    // 10
                    else if (hasPostRotation && !hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var rotation = chunkPostRotation[i].Value;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = new float4x4(rotation, float3.zero)};
                        }
                    }
                    // 11
                    else if (hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var rotation = math.mul(chunkRotations[i].Value, chunkPostRotation[i].Value);

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = new float4x4(rotation, float3.zero)};
                        }
                    }
                }
                // 101
                else if (hasAnyRotation && !hasRotationPivotTranslation && hasRotationPivot)
                {
                    // 00 - Not valid
                    // 01
                    if (!hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(RotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var rotation = chunkRotations[i].Value;
                            var pivot = chunkRotationPivots[i].Value;
                            var inversePivot = -1.0f * pivot;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = math.mul(new float4x4(rotation, pivot), float4x4.Translate(inversePivot))};
                        }
                    }
                    // 10
                    else if (hasPostRotation && !hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var rotation = chunkPostRotation[i].Value;
                            var pivot = chunkRotationPivots[i].Value;
                            var inversePivot = -1.0f * pivot;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = math.mul(new float4x4(rotation, pivot), float4x4.Translate(inversePivot))};
                        }
                    }
                    // 11
                    else if (hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var rotation = chunkPostRotation[i].Value;
                            var pivot = chunkRotationPivots[i].Value;
                            var inversePivot = -1.0f * pivot;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = math.mul(new float4x4(rotation, pivot), float4x4.Translate(inversePivot))};
                        }
                    }
                }
                // 110
                else if (hasAnyRotation && hasRotationPivotTranslation && !hasRotationPivot)
                {
                    // 00 - Not valid
                    // 01
                    if (!hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(RotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotTranslationType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var translation = chunkRotationPivotTranslations[i].Value;
                            var rotation = chunkRotations[i].Value;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = new float4x4(rotation, translation)};
                        }
                    }
                    // 10
                    else if (hasPostRotation && !hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotTranslationType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var translation = chunkRotationPivotTranslations[i].Value;
                            var rotation = chunkRotations[i].Value;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = new float4x4(rotation, translation)};
                        }
                    }
                    // 11
                    else if (hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotTranslationType, LastSystemVersion) ||
                            chunk.DidChange(RotationType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var translation = chunkRotationPivotTranslations[i].Value;
                            var rotation = math.mul(chunkRotations[i].Value, chunkPostRotation[i].Value);

                            chunkCompositeRotations[i] = new CompositeRotation
                            {Value = new float4x4(rotation, translation)};
                        }
                    }
                }
                // 111
                else if (hasAnyRotation && hasRotationPivotTranslation && hasRotationPivot)
                {
                    // 00 - Not valid
                    // 01
                    if (!hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(RotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotTranslationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var translation = chunkRotationPivotTranslations[i].Value;
                            var rotation = chunkRotations[i].Value;
                            var pivot = chunkRotationPivots[i].Value;
                            var inversePivot = -1.0f * pivot;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {
                                Value = math.mul(float4x4.Translate(translation),
                                    math.mul(new float4x4(rotation, pivot), float4x4.Translate(inversePivot)))
                            };
                        }
                    }
                    // 10
                    else if (hasPostRotation && !hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotTranslationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var translation = chunkRotationPivotTranslations[i].Value;
                            var rotation = chunkPostRotation[i].Value;
                            var pivot = chunkRotationPivots[i].Value;
                            var inversePivot = -1.0f * pivot;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {
                                Value = math.mul(float4x4.Translate(translation),
                                    math.mul(new float4x4(rotation, pivot), float4x4.Translate(inversePivot)))
                            };
                        }
                    }
                    // 11
                    else if (hasPostRotation && hasRotation)
                    {
                        var didChange = chunk.DidChange(PostRotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotTranslationType, LastSystemVersion) ||
                            chunk.DidChange(RotationPivotType, LastSystemVersion);
                        if (!didChange)
                            return;

                        for (int i = 0; i < count; i++)
                        {
                            var translation = chunkRotationPivotTranslations[i].Value;
                            var rotation = math.mul(chunkRotations[i].Value, chunkPostRotation[i].Value);
                            var pivot = chunkRotationPivots[i].Value;
                            var inversePivot = -1.0f * pivot;

                            chunkCompositeRotations[i] = new CompositeRotation
                            {
                                Value = math.mul(float4x4.Translate(translation),
                                    math.mul(new float4x4(rotation, pivot), float4x4.Translate(inversePivot)))
                            };
                        }
                    }
                }
            }
        }*/
        
        [BurstCompile]
        unsafe struct PostRotationEulerJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<RotationEulerXYZ>     RotationEulerXYZTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PostRotationEulerXYZ> PostRotationEulerXYZTypeHandle;
            public            ComponentTypeHandle<LocalTransform>       LocalTransformTypeHandleRW;
            public            uint                                      LastSystemVersion;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var rotationEulerXyzs       = chunk.GetNativeArray(ref RotationEulerXYZTypeHandle);
                var postRotationEulerXyzes  = chunk.GetNativeArray(ref PostRotationEulerXYZTypeHandle);
                var localTransforms         = chunk.GetNativeArray(ref LocalTransformTypeHandleRW);
                var count                   = chunk.Count;
                var hasPostRotationEulerXyz = chunk.Has(ref PostRotationEulerXYZTypeHandle);
                var hasRotationEulerXyz     = chunk.Has(ref RotationEulerXYZTypeHandle);
                
                if (hasPostRotationEulerXyz)
                {
                    var didChange = chunk.DidChange(ref PostRotationEulerXYZTypeHandle, LastSystemVersion);
                    if (!didChange)
                        return;
                    for (int i = 0; i < count; i++)
                    {
                        var postRotationEulerXyz = postRotationEulerXyzes[i];
                        var localTransform       = localTransforms[i];
                    
                        localTransform.Rotation = quaternion.EulerXYZ(postRotationEulerXyz.Value);
                    
                        localTransforms[i] = localTransform;
                    }
                }
                
                if (hasRotationEulerXyz)
                {
                    var didChange = chunk.DidChange(ref RotationEulerXYZTypeHandle, LastSystemVersion);
                    if (!didChange)
                        return;
                    for (int i = 0; i < count; i++)
                    {
                        var rotationEulerXyz = rotationEulerXyzs[i];
                        var localTransform   = localTransforms[i];
                    
                        localTransform.Rotation = quaternion.EulerXYZ(rotationEulerXyz.Value);
                    
                        localTransforms[i] = localTransform;
                    }
                }

            }
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new PostRotationEulerJob
            {
                RotationEulerXYZTypeHandle = SystemAPI.GetComponentTypeHandle<RotationEulerXYZ>(true),
                PostRotationEulerXYZTypeHandle = SystemAPI.GetComponentTypeHandle<PostRotationEulerXYZ>(true),
                LocalTransformTypeHandleRW     = SystemAPI.GetComponentTypeHandle<LocalTransform>(false),
                LastSystemVersion = state.LastSystemVersion
            }.Schedule(rotationEulerQuery, state.Dependency);
        }
    }
}