using Unity.Assertions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Junk.Entities.Hybrid
{
    [DisallowMultipleComponent]
    [SelectionBase]
    [RequireComponent(typeof(UIDocument))]
    public class GameMenuBehaviour : MonoBehaviour
    {
        protected VisualElement Root => uiDocument.rootVisualElement;
        public    bool          StartDisabled;

        private   UIDocument    uiDocument;
        protected World         world;
        protected EntityManager entityManager;
        protected EntityQuery   query;
        protected Entity        gameEntity;

        protected virtual void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            
            // Make it not visible on screen.
            Root.style.display = DisplayStyle.None;
        }

        private bool GetWorld()
        {
            if (world != null)
                return true;
            
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                return false;
            }
            
            world = World.DefaultGameObjectInjectionWorld;
            
            entityManager = world.EntityManager;
            query         = entityManager.CreateEntityQuery(typeof(Game));
            
            gameEntity = query.GetSingletonEntity();
            
            if (!entityManager.HasComponent<GameMenu>(gameEntity))
            {
                //Debug.Log("Adding GameMenu component");
                entityManager.AddComponent<GameMenu>(gameEntity);
                //entityManager.AddComponentObject(gameEntity, new GameMenuRef {GameMenuBehaviour = this});
                
                entityManager.SetComponentEnabled<GameMenu>(gameEntity, true);
                
                if (StartDisabled)
                {
                    entityManager.SetComponentEnabled<GameMenu>(gameEntity, false);
                }
            }

            return true;
        }
        
        protected void OnLoadPlayableSubscene()
        {
            var menu = entityManager.GetComponentData<GameMenu>(gameEntity);
            menu.PlayableSceneIsLoaded = true;
            entityManager.SetComponentData(gameEntity, menu);
        }
        
        protected void OnUnloadPlayableSubscene()
        {
            var menu = entityManager.GetComponentData<GameMenu>(gameEntity);
            menu.PlayableSceneIsLoaded = false;
            entityManager.SetComponentData(gameEntity, menu);
        }

        protected virtual void Update()
        {
            if(!GetWorld())
                return;
            
            gameEntity = query.GetSingletonEntity();
            /*
            if (query.CalculateEntityCount() < 1)
            {
                Debug.LogError("Error missing menu entity");
                return;
            }
            
            if (!entityManager.HasComponent<GameMenu>(gameEntity))
            {
                entityManager.AddComponent<GameMenu>(gameEntity);
                entityManager.AddComponentObject(gameEntity, new GameMenuRef {GameMenuBehaviour = this});
                
                entityManager.SetComponentEnabled<GameMenu>(gameEntity, true);
                
                if (StartDisabled)
                {
                    entityManager.SetComponentEnabled<GameMenu>(gameEntity, false);
                }
            }*/
        }
    }
}
