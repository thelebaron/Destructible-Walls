using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Health
{
    public static class HealthBakingUtility
    {
        public static void AddComponents(Entity entity, EntityManager entityManager, float health = 100, float maxHealth = 100, bool feedback = false)
        {
            entityManager.AddComponentData(entity, new HealthData { Value = new float3(health, maxHealth, 0) });
            entityManager.AddBuffer<HealthDamageBuffer>(entity);
            //entityManager.AddComponent<HealthState>(entity);
        }
        
        public static void AddComponents(this IBaker baker, Entity entity, float health, float maxHealth)
        {
            baker.AddComponent(entity, new HealthData { Value = new float3(health, maxHealth, 0) });
            baker.AddBuffer<HealthDamageBuffer>(entity);
            //entityManager.AddComponent<HealthState>(entity);
        }
    }
}