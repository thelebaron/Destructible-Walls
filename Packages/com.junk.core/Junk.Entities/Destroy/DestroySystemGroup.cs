namespace Junk.Entities
{
    using Unity.Entities;

    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial class DestroySystemGroup : ComponentSystemGroup
    {
    }
}