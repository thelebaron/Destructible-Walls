using Unity.Entities;
using UnityEngine.InputSystem;

namespace Junk.Entities.Systems
{
    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    public partial class GameSaveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Requires user input to save
            if(Keyboard.current == null)
                return;
            
            var gameSave = SystemAPI.GetSingleton<GameSave>();
            gameSave.DataState = DataState.None;
            
            if(Keyboard.current.f9Key.wasPressedThisFrame)
            {
                gameSave.DataState = DataState.Saving;
            }
            if(Keyboard.current.f12Key.wasPressedThisFrame)
            {
                gameSave.DataState = DataState.Loading;
            }
            
            SystemAPI.SetSingleton(gameSave);
        }
    }
}