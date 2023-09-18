using Unity.Entities;
using UnityEngine;

public class BeebeeAuthoring : MonoBehaviour
{

}

public struct Beebee : IComponentData
{
    
}

public class BeebeeBaker : Baker<BeebeeAuthoring>
{
    public override void Bake(BeebeeAuthoring authoring)
    {
        var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
        AddComponent<Beebee>(entity);
    }
}