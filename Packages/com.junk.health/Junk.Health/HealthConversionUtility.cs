
using Junk.Entities;
using Unity.Entities;
using UnityEngine;

namespace Junk.Hitpoints
{
    public static class HealthConversionUtility
    {
        public static void AddComponents(Entity entity, EntityManager entityManager, float health = 100, float maxHealth = 100, bool feedback = false)
        {
            entityManager.AddComponentData(entity, new HealthData { Value = health });

            if (!maxHealth.Equals(0))
                entityManager.AddComponentData(entity, new HealthMaximum { Value = maxHealth });

            entityManager.AddBuffer<HealthDamageBuffer>(entity);
            entityManager.AddComponent<HealthState>(entity);

            if (feedback)
            {
                entityManager.AddComponent<HealthFeedback>(entity);
            }
        }
    }
}