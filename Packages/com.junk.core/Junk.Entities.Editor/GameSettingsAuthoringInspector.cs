using Junk.Entities.Hybrid;

namespace Junk.Entities.Editor
{
    [UnityEditor.CustomEditor(typeof(GameSettingsAuthoring))]
    public class GameSettingsAuthoringInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var target = (GameSettingsAuthoring) this.target;
            GameSettingsObject.GetOrCreateSettings();
        }
    }
}