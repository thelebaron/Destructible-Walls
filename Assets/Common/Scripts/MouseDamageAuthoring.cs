using Unity.Entities;
using Unity.Physics.Extensions;
using UnityEngine;

namespace Common.Scripts
{
    public struct MouseDamage : IComponentData
    {
        public bool Damage;
    }

    public class MouseDamageBaker : Baker<MouseDamageAuthoring>
    {
        public override void Bake(MouseDamageAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MouseDamage());
        }
    }

    public class MouseDamageAuthoring : MonoBehaviour
    {
        
    }
}