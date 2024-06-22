using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Junk.Transforms
{
    /// <summary>
    /// Attaches an Entity to another Entity. NOT related to UnityEngine.Transforms.
    /// Where the attach components get processed. Maybe could be optimized to use chunk iteration.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct AttachTransformSystem : ISystem 
    {
        private EntityQuery attachTransformQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<AttachTransform>();
            builder.WithAllRW<LocalTransform>();
            
            attachTransformQuery = state.GetEntityQuery(builder);
        }

        [BurstCompile]
        struct AttachEntityTransforms : IJobChunk
        {
            [ReadOnly]                            public ComponentLookup<Disabled>            DisabledLookup;
            [NativeDisableParallelForRestriction] public ComponentLookup<LocalToWorld>        LocalToWorldLookup;
            [ReadOnly]                            public EntityTypeHandle                     EntityType;
            [ReadOnly]                            public ComponentTypeHandle<AttachTransform> AttachTransformComponentType;
            public                                       ComponentTypeHandle<LocalTransform>  LocalTransformComponentType;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities         = chunk.GetNativeArray(EntityType);
                var attachTransforms = chunk.GetNativeArray(ref AttachTransformComponentType);
                var localTransforms     = chunk.GetNativeArray(ref LocalTransformComponentType);
                
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity          = entities[i];
                    var attachTransform = attachTransforms[i];
                    var targetEntity    = attachTransform.Value;
                
                    if (DisabledLookup.HasComponent(targetEntity) || !LocalToWorldLookup.HasComponent(targetEntity))
                        continue;

                    var localToWorld = LocalToWorldLookup[targetEntity];

                    if (LocalToWorldLookup.HasComponent(entity))
                    {
                        LocalToWorldLookup[entity] = localToWorld;
                    }

                    localTransforms[i] = LocalTransform.FromMatrix(localToWorld.Value);
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            state.Dependency = new AttachEntityTransforms
            {
                DisabledLookup               = SystemAPI.GetComponentLookup<Disabled>(true),
                LocalToWorldLookup           = SystemAPI.GetComponentLookup<LocalToWorld>(),
                EntityType                   = SystemAPI.GetEntityTypeHandle(),
                AttachTransformComponentType = SystemAPI.GetComponentTypeHandle<AttachTransform>(true),
                LocalTransformComponentType  = SystemAPI.GetComponentTypeHandle<LocalTransform>()
            }.Schedule(attachTransformQuery, state.Dependency); 
        }
    }

    
}