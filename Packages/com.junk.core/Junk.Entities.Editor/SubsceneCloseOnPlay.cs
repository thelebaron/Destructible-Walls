using UnityEditor;

namespace Junk.Entities.Editor
{
    [InitializeOnLoad]
    public class SubsceneCloseOnPlay
    {
        static SubsceneCloseOnPlay()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                SubsceneControl.CloseAllScenes();
                // Add any additional logic or checks here
            }
        }
    }
}