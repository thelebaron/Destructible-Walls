using Unity.Entities;

namespace thelebaron.damage
{
    /// <summary>
    /// All this system does is add a DamageStack buffer to an entity with a health component. 
    /// It does nothing else(should this be rolled into the health system?).
    /// </summary>
    public class BootstrapDamageStackSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithStructuralChanges().WithAll<Health>().WithNone<DamageStack>().ForEach((Entity entity) =>
            {
                EntityManager.AddBuffer<DamageStack>(entity);
            }).WithoutBurst().Run();
        }
    }
}