using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Fractures
{
    [DisallowMultipleComponent]
    public class ConnectedNodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public List<Transform> Connections = new List<Transform>();
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            
        }
    }
}