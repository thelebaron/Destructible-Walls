using Unity.Entities;
using UnityEngine;

namespace Destructibles
{
    public class AnchorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Anchor());
        }
    }
    
    /// <summary>
    /// An anchor prevents a physicsvelocity from being added to an entity. 
    /// </summary>
    public struct Anchor : IComponentData
    {

    }
}