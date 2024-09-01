using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Junk.Transforms
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.BakingSystem)]
    public partial struct ReparentSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            /*var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (reparent, entity) in SystemAPI.Query<RefRO<Reparent>>().WithEntityAccess())
            {
                ecb.SetComponent(reparent.ValueRO.Entity, new Parent{Value = reparent.ValueRO.Target});
                ecb.RemoveComponent<Reparent>(entity);
            }
            ecb.Playback(state.EntityManager);*/
        }
    }
}