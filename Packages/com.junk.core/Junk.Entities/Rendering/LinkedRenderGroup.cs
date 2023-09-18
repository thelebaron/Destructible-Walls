using System.Diagnostics.CodeAnalysis;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace Junk.Entities.Rendering
{
    /// <summary>
    /// A buffer that contains a list of entities that will have a DisableRendering component added
    /// or removed depending on the existence of the tag on the entity containing the buffer.
    /// </summary>
    public struct LinkedRenderGroup : IBufferElementData
    {
        public Entity Value;
        
        //implicit operator
        public static implicit operator Entity(LinkedRenderGroup linkedRenderGroup)
        {
            return linkedRenderGroup.Value;
        }
        //implicit operator
        public static implicit operator LinkedRenderGroup(Entity entity)
        {
            return new LinkedRenderGroup {Value = entity};
        }
    }
    
    /// <summary>
    /// If performance issues using brute force, add a component to the entity containing this buffer
    /// to bypass per frame checks.
    /// Note system not modified to take this into account yet.
    /// </summary>
    struct LinkedRenderState : IComponentData
    {
        public bool Enabled;
    }
    
    /// <summary>
    /// Auto adds or removes disable render tags to entities within a dynamic buffer.
    /// At large scale this could be a performance hit, and might need to be optimized.
    /// </summary>
    
    [BurstCompile]
    public partial struct LinkedRenderSystem : ISystem
    {
        private EntityQuery renderGroupQuery;

        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LinkedRenderGroup>();
            renderGroupQuery = state.GetEntityQuery(builder);
        }

        [BurstCompile]
        private struct UpdateLinkedRenderEntitiesJob : IJobChunk
        {
            public            EntityCommandBuffer.ParallelWriter        CommandBuffer;
            [ReadOnly] public EntityTypeHandle                          EntityType;
            [ReadOnly] public BufferTypeHandle<LinkedRenderGroup>       LinkedRenderGroupType;
            [ReadOnly] public ComponentLookup<DisableRendering> DisableRenderingData;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities           = chunk.GetNativeArray(EntityType);
                var linkedRenderGroups = chunk.GetBufferAccessor(ref LinkedRenderGroupType);
                
                // loop through all entities in the chunk
                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var linkedRenderGroup = linkedRenderGroups[i];

                    // if the entity has a disable rendering component, check all linked entities for the tag and add it if it doesn't exist
                    if(entity.HasComponent<DisableRendering>(DisableRenderingData))
                    {
                        for (int a = 0; a < linkedRenderGroup.Length; a++)
                        {
                            var linkedRenderEntity = linkedRenderGroup[a].Value;
                            if (!linkedRenderEntity.HasComponent<DisableRendering>(DisableRenderingData))
                            {
                                CommandBuffer.AddComponent(unfilteredChunkIndex, linkedRenderEntity, new DisableRendering());
                            }
                        }
                    }
                    
                    // if the entity doesn't have a disable rendering component, check all linked entities for the tag and remove it if it exists
                    if(!entity.HasComponent<DisableRendering>(DisableRenderingData))
                    {
                        for (int b = 0; b < linkedRenderGroup.Length; b++)
                        {
                            var linkedRenderEntity = linkedRenderGroup[b].Value;
                            if (linkedRenderEntity.HasComponent<DisableRendering>(DisableRenderingData))
                            {
                                CommandBuffer.RemoveComponent<DisableRendering>(unfilteredChunkIndex, linkedRenderEntity);
                            }
                        }
                    }
                }
            }
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            state.Dependency = new UpdateLinkedRenderEntitiesJob
            {
                CommandBuffer         = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                EntityType            = SystemAPI.GetEntityTypeHandle(),
                LinkedRenderGroupType = SystemAPI.GetBufferTypeHandle<LinkedRenderGroup>(true),
                DisableRenderingData  = SystemAPI.GetComponentLookup<DisableRendering>(true)
            }.Schedule(renderGroupQuery, state.Dependency);
        }
    }
}