namespace Junk.Entities
{
    using Unity.Entities;

    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public partial class DestroySystemGroup : ComponentSystemGroup
    {
    }
}