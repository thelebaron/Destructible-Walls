using Junk.Health;
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
        AddComponent(entity, new HealthData { Value = new float3(authoring.Health, authoring.MaxHealth, 0f) });
        AddBuffer<HealthDamageBuffer>(entity);
        //AddComponent<HealthState>(entity);
        
    }
}