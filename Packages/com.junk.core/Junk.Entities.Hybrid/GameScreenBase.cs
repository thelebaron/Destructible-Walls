using UnityEngine;
using UnityEngine.UIElements;

namespace Junk.Entities.Hybrid
{
    public abstract class GameScreenBase : MonoBehaviour
    {
        public static GameScreenBase Instance;
        
        protected virtual void OnEnable()
        {
            Instance = this;
        }
        
        public static void SetMenuEnabled(bool show)
        {
            Instance.ShowOrHideMenu(show);
        }

        protected abstract void ShowOrHideMenu(bool show);
    }
}