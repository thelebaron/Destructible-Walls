using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    public struct GameMenu : IComponentData, IEnableableComponent
    {
        public bool PlayableSceneIsLoaded; // bit of a hack: if entity scene is not loaded,
                                           // when we hide the menu it shows just debug crap
                                           // so we need to check if any desired subscene
                                           // is loaded
        public bool IsEnabled;
    }

    public struct Game : IComponentData
    {
        public GameState         State;
        public float             Time;
        public float             TimeScale;
        public bool              Reload;
        public int               CameraMode;
        public int               ResolutionX;
        public int               ResolutionY;
        public float             LimitInputTime;
        public MenuGameplayState MenuState;
        public MouseState        Mouse;
        
        public enum MouseState
        {
            LockedAndHidden,
            LockedAndVisible,
            UnlockedAndVisible,
        }
        
        public enum MenuGameplayState
        {
            GameMenu,
            Gameplay,
        }
        
        public enum GameState
        {
            Default,
            Splash,
            Play,
            Pause,
            GameOver
        }
    }
    
    
    


}