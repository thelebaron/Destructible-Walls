using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    /// <summary>
    /// An authoring component for the GameObject that will be stored in a component.
    /// </summary>
    public class GameObjectPooledPrefabAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
    }
    public class GameObjectPooledPrefabBaker : Baker<GameObjectPooledPrefabAuthoring>
    {
        public override void Bake(GameObjectPooledPrefabAuthoring authoring)
        {
            AddComponentObject( new GameObjectPooledPrefab
            {
                Prefab = authoring.Prefab
            });
        }
    }
    /// <summary>
    /// A "GameObjectPrefab" is a component that stores a gameobject that will be used in an entity scene, that has its instantiation/destruction
    /// managed by user made systems, ie a way to bypass the buggy companion system. It also lives outside of the standard entity instantiation/destruction
    /// so it can be pooled for reuse.
    /// </summary>
    public class GameObjectPooledPrefab : IComponentData
    {
        public GameObject Prefab;
    }
    
    /*
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class GameObjectPooledPrefabDeclareSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GameObjectPooledPrefabAuthoring component) =>
            {
                if(component.Prefab == null)
                    Debug.LogError("Prefab is null on "+ component.gameObject.name);
            }).WithStructuralChanges().Run();
        }
    }
    
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    public class GameObjectPooledPrefabConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GameObjectPooledPrefabAuthoring component) =>
            {
                var entity = GetPrimaryEntity(component);
                DstEntityManager.AddComponentObject(entity, new GameObjectPooledPrefab
                {
                    Prefab = component.Prefab
                });
            });
        }
    }*/
}