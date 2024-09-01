using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Transforms
{
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.BakingSystem)]
    public partial struct SinMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (move, localTransform) in SystemAPI.Query<RefRW<SinMove>,RefRW<LocalTransform>>())
            {
                //localTransform.ValueRW.Position;
                var   time = (float)SystemAPI.Time.ElapsedTime;
                float newY = localTransform.ValueRW.Position.y + math.sin(time * move.ValueRW.Frequency) *  move.ValueRW.Amplitude;
                localTransform.ValueRW.Position = new float3(localTransform.ValueRW.Position.x, newY, localTransform.ValueRW.Position.z);
            }
        }
    }
}