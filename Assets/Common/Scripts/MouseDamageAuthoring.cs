using Unity.Entities;
using Unity.Physics.Extensions;
using UnityEngine;

namespace Common.Scripts
{
    public struct MouseDamage : IComponentData
    {
        public bool Damage;
    }
    
    public class MouseDamageAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new MouseDamage());
            //dstManager.AddComponentData(entity, new MousePick());
        }
    }
}