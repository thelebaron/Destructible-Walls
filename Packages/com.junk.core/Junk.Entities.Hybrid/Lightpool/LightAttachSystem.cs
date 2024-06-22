using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Entities
{
    public partial struct LightAttachSystem : ISystem
    {
        private EntityQuery poolQuery;
        private EntityQuery disconnectedLightQuery;
        private EntityQuery connectedLightQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform>();
            builder.WithDisabled<PoolEntityReference>();
            poolQuery = state.GetEntityQuery(builder);
            builder.Dispose();
            
            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform>();
            builder.WithAll<UnmanagedLightData>();
            builder.WithPresent<UnmanagedLightReference>();
            disconnectedLightQuery = state.GetEntityQuery(builder);
            builder.Dispose();

            state.GetComponentTypeHandle<PoolEntityReference>();

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform>();
            builder.WithAll<UnmanagedLightData>();
            builder.WithAll<UnmanagedLightReference>();
            connectedLightQuery    = state.GetEntityQuery(builder);
            builder.Dispose();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Unattach any lights that have had their unmanaged equivalents destroyed
            foreach (var (reference, returnToPool, entity) in SystemAPI.Query<RefRO<PoolEntityReference>, EnabledRefRW<ReturnToPool>>().WithEntityAccess())
            {
                if (!state.EntityManager.Exists(reference.ValueRO.Entity))
                {
                    //SystemAPI.SetComponentEnabled<ReturnToPool>(entity, true);
                    returnToPool.ValueRW = false;
                }
            }

            var lightPoolEntities = poolQuery.ToEntityListAsync(state.WorldUpdateAllocator, out var gatherEntitiesDependency);
            state.Dependency = JobHandle.CombineDependencies(state.Dependency, gatherEntitiesDependency);
            
            //var lightPoolEntities = poolQuery.ToEntityArray(Allocator.TempJob);
            /*var list              = new NativeList<Entity>(Allocator.TempJob);
            
            state.Dependency = new ArrayToList
            {
                Array = lightPoolEntities,
                List  = list
            }.Schedule(state.Dependency);*/

            state.Dependency = new AttachLightJob
            {
                LightPoolEntities             = lightPoolEntities,//list,
                UnmanagedLightReferenceLookup = SystemAPI.GetComponentLookup<UnmanagedLightReference>(),
                PoolEntityReferenceLookup     = SystemAPI.GetComponentLookup<PoolEntityReference>()
            }.Schedule(disconnectedLightQuery, state.Dependency);
            
            state.Dependency = new LightTransformJob
            {
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
            }.Schedule(state.Dependency);
            
            //list.Dispose(state.Dependency);
        }

        [BurstCompile]
        private struct ArrayToList : IJob
        {
            public NativeArray<Entity> Array;
            public NativeList<Entity>  List;
            public void Execute()
            {
                for (int i = 0; i < Array.Length; i++)
                {
                    List.Add( Array[i] );
                }
            }
        }
        
        [BurstCompile]
        //[WithDisabled(typeof(UnmanagedLightReference))]
        private partial struct AttachLightJob : IJobEntity
        {
            public NativeList<Entity> LightPoolEntities;
            public ComponentLookup<UnmanagedLightReference> UnmanagedLightReferenceLookup;
            public ComponentLookup<PoolEntityReference>     PoolEntityReferenceLookup;
            
            /*note this might throw errors as you also need ref UnmanagedLightReference reference  to write with it and currently unfixed by unity*/
            public void Execute(Entity entity /*ref EnabledRefRW<UnmanagedLightReference> reference */)
            {
                if (UnmanagedLightReferenceLookup.IsComponentEnabled(entity)) 
                    return;
                
                // reverse loop
                for (int i = LightPoolEntities.Length - 1; i >= 0; i--)
                {
                    var poolEntity     = LightPoolEntities[i];
                    var lightReference = UnmanagedLightReferenceLookup[entity];
                    lightReference.Entity                 = poolEntity;
                    UnmanagedLightReferenceLookup[entity] = lightReference;
                    UnmanagedLightReferenceLookup.SetComponentEnabled(entity, true);
                    
                    var attachedToEntity = PoolEntityReferenceLookup[poolEntity];
                    attachedToEntity.Entity               = entity;
                    PoolEntityReferenceLookup[poolEntity] = attachedToEntity;
                    PoolEntityReferenceLookup.SetComponentEnabled(poolEntity, true);
                    
                    LightPoolEntities.RemoveAtSwapBack(i);
                    break;
                }
            }
        }
        
        [BurstCompile]
        private partial struct LightTransformJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            
            public void Execute(Entity entity, in UnmanagedLightReference reference, in LocalToWorld localToWorld)
            {
                LocalTransformLookup[reference.Entity] = LocalTransform.FromPositionRotation(localToWorld.Position, localToWorld.Rotation);
            }
        }
    }
}