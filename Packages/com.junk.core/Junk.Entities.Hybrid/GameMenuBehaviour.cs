using System;
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
        protected World         World;
        protected EntityManager EntityManager;
        protected EntityQuery   Query;
        protected Entity        GameEntity;

        protected virtual void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            
            // Make it not visible on screen.
            Root.style.display = DisplayStyle.None;
        }

        protected void OnDestroy()
        {
            Query.Dispose();
        }

        private bool GetWorld()
        {
            if (World != null)
                return true;
            
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                return false;
            }
            
            World = World.DefaultGameObjectInjectionWorld;
            
            EntityManager = World.EntityManager;
            Query         = EntityManager.CreateEntityQuery(typeof(Game));
            
            GameEntity = Query.GetSingletonEntity();
            
            if (!EntityManager.HasComponent<GameMenu>(GameEntity))
            {
                //Debug.Log("Adding GameMenu component");
                EntityManager.AddComponent<GameMenu>(GameEntity);
                //entityManager.AddComponentObject(gameEntity, new GameMenuRef {GameMenuBehaviour = this});
                
                EntityManager.SetComponentEnabled<GameMenu>(GameEntity, true);
                
                if (StartDisabled)
                {
                    EntityManager.SetComponentEnabled<GameMenu>(GameEntity, false);
                }
            }

            return true;
        }
        
        protected void OnLoadPlayableSubscene()
        {
            var menu = EntityManager.GetComponentData<GameMenu>(GameEntity);
            menu.PlayableSceneIsLoaded = true;
            EntityManager.SetComponentData(GameEntity, menu);
        }
        
        protected void OnUnloadPlayableSubscene()
        {
            var menu = EntityManager.GetComponentData<GameMenu>(GameEntity);
            menu.PlayableSceneIsLoaded = false;
            EntityManager.SetComponentData(GameEntity, menu);
        }

        protected virtual void Update()
        {
            if(!GetWorld())
                return;
            
            GameEntity = Query.GetSingletonEntity();
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
