using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using Unity.Transforms;
using UnityEngine;

public class BeebeeAuthoring : MonoBehaviour
{

}

public struct Beebee : IComponentData
{
    
}

public struct BeebeeElement : IBufferElementData
{
    public Entity Value;
}

public class BeebeeBaker : Baker<BeebeeAuthoring>
{
    public override void Bake(BeebeeAuthoring authoring)
    {
        var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
        AddComponent<Beebee>(entity);
        AddBuffer<BeebeeElement>(entity);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
public partial struct BeebeeAuthoringSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
            
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (localTransform, additionalEntitiesBakingData, beebeeElements, entity) in 
                 SystemAPI.Query<LocalTransform, DynamicBuffer<AdditionalEntitiesBakingData>, DynamicBuffer<BeebeeElement>>().WithAll<Beebee>()
                     .WithEntityAccess().WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities))
        {
            var additionalEntitiesBakingDataArray = additionalEntitiesBakingData.AsNativeArray();
            for (var index = 0; index < additionalEntitiesBakingDataArray.Length; index++)
            {
                var additionalEntity = additionalEntitiesBakingDataArray[index];
                beebeeElements.Add(new BeebeeElement
                {
                    Value = additionalEntity.Value
                });
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}