using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Junk.Entities
{
    public struct GameSettings : IComponentData
    {
        public float               Volume;
        public int                 Number;
        public FixedString128Bytes SomeString;
    }
    
    public partial struct GameSettingsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // change volume
            
            foreach (var settings in SystemAPI.Query<RefRO<GameSettings>>().WithChangeFilter<GameSettings>())
            {
                
            }
        }
    }
}