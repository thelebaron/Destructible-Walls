using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Entities
{
    
    public class LinkedInstantiationAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
    }
    
    
    public class LinkedInstantiationBaker : Baker<LinkedInstantiationAuthoring>
    {
        public override void Bake(LinkedInstantiationAuthoring authoring)
        {
            
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new InstantiateLinkedPrefab { Prefab = GetEntity(authoring.Prefab) });
        }
    }

    /// <summary>
    /// When the entity is instantiated, instantiate a prefab.
    /// </summary>
    public struct InstantiateLinkedPrefab : IComponentData
    {
        public Entity Prefab;
    }
    
    public struct InstantiatedPrefab : IComponentData { }  
    
    [BurstCompile]    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    public partial struct LinkedInstantiationSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        struct LinkedInstantiationJob : IJobChunk
        {
            public            EntityCommandBuffer                          CommandBuffer;
            [ReadOnly] public EntityTypeHandle                             EntityType;
            [ReadOnly]  public ComponentTypeHandle<InstantiateLinkedPrefab> InstantiateLinkedPrefabTypeHandle;
            [ReadOnly]  public ComponentLookup<LocalToWorld>        LocalToWorldDataFromEntity;
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities                   = chunk.GetNativeArray(EntityType);
                var linkedInstantiationPrefabs = chunk.GetNativeArray(ref InstantiateLinkedPrefabTypeHandle);
                
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var linkedInstantiationPrefab = linkedInstantiationPrefabs[i];
                    var prefab = linkedInstantiationPrefab.Prefab;
                    
                    var position = LocalToWorldDataFromEntity[entity].Position;
                    CommandBuffer.AddComponent(entity, new InstantiatedPrefab());
                    var newEntity = CommandBuffer.Instantiate(prefab);
                    
                    // was just set translation so unsure if this is valid
                    // todo transformv2
                    CommandBuffer.SetComponent(newEntity,  LocalTransform.FromMatrix(LocalToWorldDataFromEntity[entity].Value)); 
                }
            }
        }

        public void OnCreate(ref  SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<InstantiateLinkedPrefab,LocalToWorld>();
            builder.WithNone<InstantiatedPrefab>();
            query = state.GetEntityQuery(builder);
        }

        public void OnDestroy(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref  SystemState state)
        {
            state.Dependency = new LinkedInstantiationJob
            {
                CommandBuffer                     = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                EntityType                        = SystemAPI.GetEntityTypeHandle(),
                InstantiateLinkedPrefabTypeHandle = SystemAPI.GetComponentTypeHandle<InstantiateLinkedPrefab>(true),
                LocalToWorldDataFromEntity        = SystemAPI.GetComponentLookup<LocalToWorld>(true)
            }.Schedule(query, state.Dependency);
        }
    }
}
