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
        public UIDocument    UIDocument;
        public VisualElement Root;
        public bool          StartDisabled;
        private void Start()
        {
            Assert.IsNotNull(UIDocument);
            Root = UIDocument.rootVisualElement;
            //Root.SetEnabled(false);
            
            // Make it not visible on screen.
            Root.style.display = DisplayStyle.None;
       
            // Make it visible on screen.
            //Root.style.display = DisplayStyle.Flex;

        }

        private void Update()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                return;
            }
            
            var entityManager = world.EntityManager;
            var query         = entityManager.CreateEntityQuery(typeof(Game));

            if (query.CalculateEntityCount() < 1)
            {
                Debug.Log("No entity");
                return;
            }
            
            var gameEntity = query.GetSingletonEntity();
            if (!entityManager.HasComponent<GameMenu>(gameEntity))
            {
                entityManager.AddComponent<GameMenu>(gameEntity);
                entityManager.AddComponentObject(gameEntity, new GameMenuRef {GameMenuBehaviour = this});
                
                entityManager.SetComponentEnabled<GameMenu>(gameEntity, true);
                
                if (StartDisabled)
                {
                    entityManager.SetComponentEnabled<GameMenu>(gameEntity, false);
                }
            }
            
            enabled = false;
        }

        
    }
}
