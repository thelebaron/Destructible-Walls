using Junk.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Junk.Fracture
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial struct FractureRootSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (graph, entity) in SystemAPI.Query<DynamicBuffer<FractureGraph>>()
                         .WithOptions(EntityQueryOptions.IncludePrefab).WithEntityAccess())
            {
                // Setup go proxies
                var graphArray = graph.AsNativeArray();

                for (int i = 0; i < graphArray.Length; i++)
                {
                    var node      = graphArray[i].Node;
                    var isFractured = SystemAPI.IsComponentEnabled<Fractured>(node);

                    if (isFractured)
                    {
                        
                    }
                }
            }

            ecb.Playback(state.EntityManager);
        }
    }
}