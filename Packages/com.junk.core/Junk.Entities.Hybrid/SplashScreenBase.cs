using Unity.Assertions;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Junk.Entities.Hybrid
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class SplashScreenBase  : MonoBehaviour, InputActions.IInterfaceActions
    {
        [SerializeField] private UIDocument    UIDocument;
        [SerializeField] private bool          OpenMenuOnSkip = true;
        private                  VisualElement root;
        private                  Keyboard      keyboard;
        private                  InputActions  inputActions;
        public                   bool          StartDisabled;

        private void Start()
        {
            inputActions = new InputActions();
            inputActions.Interface.SetCallbacks(this);
            inputActions.Enable();
            
            if(UIDocument == null)
                UIDocument = GetComponent<UIDocument>();
            Assert.IsNotNull(UIDocument);
            root = UIDocument.rootVisualElement;
            root.style.display = DisplayStyle.Flex;
            if (StartDisabled)
                gameObject.SetActive(false);
        }

        private void Skip()
        {
            root.style.display = DisplayStyle.None;
            inputActions.Disable();
            enabled = false;

            if (OpenMenuOnSkip)
            {
                var world = World.DefaultGameObjectInjectionWorld;
                if (world != null)
                {
                    var query               = world.EntityManager.CreateEntityQuery(typeof(Junk.Entities.Game));
                    var gameSingletonEntity = query.GetSingletonEntity();
                    if (world.EntityManager.HasComponent<GameMenu>(gameSingletonEntity))
                    {
                        world.EntityManager.SetComponentEnabled<GameMenu>(gameSingletonEntity, true);
                        
                        if (StartDisabled)
                        {
                            world.EntityManager.SetComponentEnabled<GameMenu>(gameSingletonEntity, false);
                        }
                    }
                }
            }
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            Skip();
        }

        public void OnAnyKey(InputAction.CallbackContext context)
        {
            Skip();
        }

        public void OnQuitGame(InputAction.CallbackContext           context)
        {
            
        }
    }
}