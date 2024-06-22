using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Junk.Entities.Hybrid
{
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct LifetimeBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (tracerSpawner, entity) in SystemAPI.Query<TimeDestroy>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities))
            {
                if (!SystemAPI.HasComponent<Destroy>(entity))
                {
                    ecb.AddComponent<Destroy>(entity);
                    ecb.SetComponentEnabled<Destroy>(entity, false);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        
    }
}