using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable RedundantExplicitArrayCreation

namespace Junk.Fracture.Hybrid
{
    [RequireMatchingQueriesForUpdate]
    //[UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct FractureRenderBakingSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<FractureRenderData>();
            builder.WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab);
            query = builder.Build(ref state);
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity     = entities[i];
                var renderData = state.EntityManager.GetComponentObject<FractureRenderData>(entity);
                // Rendering
                uint renderlayerMask = 1 << 0;
                var  desc            = new RenderMeshDescription(ShadowCastingMode.On, true, MotionVectorGenerationMode.Object, 0, renderlayerMask, LightProbeUsage.BlendProbes);
                RenderMeshUtility.AddComponents(entity, state.EntityManager, desc, renderData.RenderMeshArray, renderData.MaterialMeshInfo);
            }

            entities.Dispose();
        }
    }
}