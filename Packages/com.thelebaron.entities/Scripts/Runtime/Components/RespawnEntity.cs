using Unity.Entities;

namespace thelebaron.bee
{
    [GenerateAuthoringComponent]
    public struct RespawnEntity : IComponentData
    {
        // The entity whos transform we will mimic by "attaching" to.
        public Entity Entity;
    }
}