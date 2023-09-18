using Junk.Hitpoints;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class HealthAuthoring : MonoBehaviour
{
    public int Health = 50;
    public int MaxHealth = 50;
}

// still wip
public class HealthBaker : Baker<HealthAuthoring>
{
    public override void Bake(HealthAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new HealthData { Value = authoring.Health });

        if (!authoring.MaxHealth.Equals(0))
            AddComponent(entity, new HealthMaximum { Value = authoring.MaxHealth });

        AddBuffer<HealthDamageBuffer>(entity);
        AddComponent<HealthState>(entity);
        AddComponent<HealthFeedback>(entity);
        //if (feedback)
        {
            AddComponent<HealthFeedback>(entity);
        }
        
    }
}