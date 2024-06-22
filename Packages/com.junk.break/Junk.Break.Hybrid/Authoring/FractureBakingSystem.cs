using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.Break.Hybrid
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct FractureBakingSystem : ISystem
    {
        private EntityQuery query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            //builder.WithAll<FractureBaker.FractureChild>();
            //builder.WithAll<FractureBaker.Fracture>();
            builder.WithAll<FractureRenderData>();
            builder.WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab);
            query = builder.Build(ref state);
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.TempJob);
            
            for (int i = 0; i < entities.Length; i++)
            {
                var entity     = entities[i];
                var renderData = state.EntityManager.GetComponentObject<FractureRenderData>(entity);
                // Rendering
                var desc             = new RenderMeshDescription(ShadowCastingMode.On, true, MotionVectorGenerationMode.Object, 0);
                var renderMeshArray  = new RenderMeshArray(new Material[2] { renderData.OutsideMaterial,renderData.InsideMaterial }, new Mesh[1]{ renderData.Mesh }); 
                var materialIndex    = renderData.MaterialIndex;
                var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(materialIndex, 0, renderData.SubMeshIndex);
                RenderMeshUtility.AddComponents(entity, state.EntityManager, desc, renderMeshArray, materialMeshInfo);
            }
            
            entities.Dispose();
        }
    }
}