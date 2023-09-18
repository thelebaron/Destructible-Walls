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
    //[UpdateAfter(typeof(WorldToLocalSystem))] // transformv2
    public partial struct AttachTransformSystem : ISystem 
    {
        private EntityQuery attachQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<AttachTransform>();
            builder.WithAllRW<LocalTransform>();
            
            attachQuery = state.GetEntityQuery(builder);
        }

        [BurstCompile]
        struct AttachEntityTransforms : IJobChunk
        {
            [ReadOnly]                            public ComponentLookup<Disabled>            DisabledCdfe;
            [NativeDisableParallelForRestriction] public ComponentLookup<LocalToWorld>        LocalToWorldLookup;
            [ReadOnly]                            public EntityTypeHandle                     EntityTypeHandle;
            [ReadOnly]                            public ComponentTypeHandle<AttachTransform> AttachTransformComponentTypeHandle;
            public                                       ComponentTypeHandle<LocalTransform>  LocalTransformComponentTypeHandle;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities         = chunk.GetNativeArray(EntityTypeHandle);
                var attachTransforms = chunk.GetNativeArray(ref AttachTransformComponentTypeHandle);
                var localTransforms     = chunk.GetNativeArray(ref LocalTransformComponentTypeHandle);
                
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity          = entities[i];
                    var attachTransform = attachTransforms[i];
                    var targetEntity    = attachTransform.Value;
                
                    if (DisabledCdfe.HasComponent(targetEntity) || !LocalToWorldLookup.HasComponent(targetEntity))
                        return;

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
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            state.Dependency = new AttachEntityTransforms
            {
                DisabledCdfe                       = SystemAPI.GetComponentLookup<Disabled>(true),
                LocalToWorldLookup                 = SystemAPI.GetComponentLookup<LocalToWorld>(),
                EntityTypeHandle                   = SystemAPI.GetEntityTypeHandle(),
                AttachTransformComponentTypeHandle = SystemAPI.GetComponentTypeHandle<AttachTransform>(true),
                LocalTransformComponentTypeHandle  = SystemAPI.GetComponentTypeHandle<LocalTransform>()
            }.Schedule(attachQuery, state.Dependency); 
            
            /*// Note cannot switch to HasComponent yet as unsure how to enable write access.
            var localToWorldCdfe = GetComponentLookup<LocalToWorld>();
            var disabledCdfe = GetComponentLookup<Disabled>(true);
            
            // Entities Codegen
            //Dependency = 
            Entities
            .WithName("AttachEntityJob")
            .WithNativeDisableParallelForRestriction(localToWorldCdfe)
            .WithReadOnly(disabledCdfe)
            .ForEach((Entity entity, ref AttachTransform attachEntity, ref LocalTransform translation, ref Rotation rotation) =>
            {
                var targetEntity = attachEntity.Value;
                
                if (disabledCdfe.HasComponent(targetEntity) || !localToWorldCdfe.HasComponent(targetEntity))
                    return;

                var localToWorld = localToWorldCdfe[targetEntity];

                translation.Value = localToWorld.Position;
                rotation.Value = new quaternion(localToWorld.Value);

                if (localToWorldCdfe.HasComponent(entity))
                {
                    localToWorldCdfe[entity] = localToWorld;
                }
            }).WithBurst().Run();
            //}).WithBurst().ScheduleParallel(Dependency);*/
        }
    }

    
}