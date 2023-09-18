using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    public struct GameMenu : IComponentData, IEnableableComponent
    {
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
        public float             InputLimiter;
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