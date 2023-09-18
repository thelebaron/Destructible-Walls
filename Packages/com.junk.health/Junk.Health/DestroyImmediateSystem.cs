using Junk.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace CyberJunk.Health
{
    /*
    //
    public struct DestroyImmediate : IComponentData
    {
        
    }
    
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PreSimulationStructuralChangeSystemGroup))]
    public partial struct DestroyImmediateSystem : ISystem
    {
        private EntityQuery query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DestroyImmediate>()
                .WithOptions(EntityQueryOptions.Default);
            query = state.GetEntityQuery(builder);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.DestroyEntity(query);
        }
    }*/
}
