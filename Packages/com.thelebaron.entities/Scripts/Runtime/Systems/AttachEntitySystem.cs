using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace thelebaron.bee
{
    /// <summary>
    /// Attaches an Entity to another Entity. NOT related to UnityEngine.Transforms.
    /// Where the attach components get processed. Maybe could be optimized to use chunk iteration.
    /// </summary>
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(EndFrameWorldToLocalSystem))]
    public class AttachTransformSystem : SystemBase
    {
        private EntityQuery attachQuery;
        
        protected override void OnCreate()
        {
            attachQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<AttachTransform>(),
                    ComponentType.ReadWrite<Translation>(),
                    ComponentType.ReadWrite<Rotation>(),
                }
            });
        }

        [BurstCompile]
        struct AttachEntityTransforms : IJobChunk
        {
            [ReadOnly]                            public ComponentDataFromEntity<Disabled>     DisabledCdfe;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<LocalToWorld> LocalToWorldCdfe;
            [ReadOnly]                            public EntityTypeHandle                      EntityTypeHandle;
            [ReadOnly]                            public ComponentTypeHandle<AttachTransform>  AttachTransformComponentTypeHandle;
            public                                       ComponentTypeHandle<Translation>      TranslationComponentTypeHandle;
            public                                       ComponentTypeHandle<Rotation>         RotationComponentTypeHandle;
        
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkEntities = chunk.GetNativeArray(EntityTypeHandle);
                var chunkAttachTransforms = chunk.GetNativeArray(AttachTransformComponentTypeHandle);
                var chunkTranslations = chunk.GetNativeArray(TranslationComponentTypeHandle);
                var chunkRotation = chunk.GetNativeArray(RotationComponentTypeHandle);
                
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = chunkEntities[i];
                    var attachTransform = chunkAttachTransforms[i];
                    var targetEntity = attachTransform.Value;
                
                    if (DisabledCdfe.HasComponent(targetEntity) || !LocalToWorldCdfe.HasComponent(targetEntity))
                        return;

                    var localToWorld = LocalToWorldCdfe[targetEntity];
                    var translation = new Translation{ Value = localToWorld.Position };
                    var rotation = new Rotation{ Value = new quaternion(localToWorld.Value) };

                    if (LocalToWorldCdfe.HasComponent(entity))
                    {
                        LocalToWorldCdfe[entity] = localToWorld;
                    }
                    
                    chunkTranslations[i] = translation;
                    chunkRotation[i] = rotation;
                }
                
                
            }
        }
        
        protected override void OnUpdate()
        {
            Dependency = new AttachEntityTransforms
            {
                DisabledCdfe                       = GetComponentDataFromEntity<Disabled>(true),
                LocalToWorldCdfe                   = GetComponentDataFromEntity<LocalToWorld>(),
                EntityTypeHandle                   = GetEntityTypeHandle(),
                AttachTransformComponentTypeHandle = GetComponentTypeHandle<AttachTransform>(true),
                TranslationComponentTypeHandle     = GetComponentTypeHandle<Translation>(),
                RotationComponentTypeHandle        = GetComponentTypeHandle<Rotation>()
            }.Schedule(attachQuery, Dependency); 
            
            /*// Note cannot switch to HasComponent yet as unsure how to enable write access.
            var localToWorldCdfe = GetComponentDataFromEntity<LocalToWorld>();
            var disabledCdfe = GetComponentDataFromEntity<Disabled>(true);
            
            // Entities Codegen
            //Dependency = 
            Entities
            .WithName("AttachEntityJob")
            .WithNativeDisableParallelForRestriction(localToWorldCdfe)
            .WithReadOnly(disabledCdfe)
            .ForEach((Entity entity, ref AttachTransform attachEntity, ref Translation translation, ref Rotation rotation) =>
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