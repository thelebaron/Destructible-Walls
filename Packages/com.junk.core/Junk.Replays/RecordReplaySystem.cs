using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;

namespace Junk.Replays
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct RecordReplaySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var time in SystemAPI.Query<DynamicBuffer<RecordedDeltaTime>>())
            {
                time.Add(new RecordedDeltaTime { Value = SystemAPI.Time.DeltaTime });
            }
        }
    }
    
    public struct RecordedDeltaTime : IBufferElementData
    {
        public float Value;
    }
}
