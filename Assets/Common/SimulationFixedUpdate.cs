using Unity.Entities;

namespace Game
{
    public class SimulationFixedUpdate : SystemBase
    {
        private void SetFixedUpdate()
        {
            FixedRateUtils.EnableFixedRateWithCatchUp(World.GetOrCreateSystem<SimulationSystemGroup>(), Time.fixedDeltaTime);
            Enabled = false;
        }
        
        protected override void OnUpdate()
        {
            SetFixedUpdate();
        }
    }
}