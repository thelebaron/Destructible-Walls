using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// All this system does is add a HealthBuffer buffer to an entity with a health component. 
    /// It does nothing else(should this be rolled into the health system?).
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class HealthBufferInitializationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .WithAny<Health>()
                .WithAny<HealthLink>()
                .WithNone<HealthFrameBuffer>().ForEach((Entity entity) =>
            {
                EntityManager.AddBuffer<HealthFrameBuffer>(entity);
            }).WithoutBurst().Run();
        }
    }
}