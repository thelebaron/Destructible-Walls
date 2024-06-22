#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.VFX;

namespace Junk.Entities.Hybrid
{
    public class VisualEffectResources : ScriptableObject
    {
        public const string k_settingsPath = "Assets/Settings/VisualEffectResources.asset";
        
        [SerializeField] private VisualEffect sparkEffect;
        public                   VisualEffect SparkEffect => sparkEffect;
        [SerializeField] private VisualEffect smokeEffect;
        public                   VisualEffect SmokeEffect => smokeEffect;
        [SerializeField] private VisualEffect bloodSprayEffect;
        public                   VisualEffect BloodSprayEffect => bloodSprayEffect;
        [SerializeField] private VisualEffect bloodHeadshotEffect;
        public                   VisualEffect BloodHeadshotEffect => bloodHeadshotEffect;
        
        // Testing
        [SerializeField] private VisualEffect trailMoverEffect;
        public                   VisualEffect TrailMoverEffect => trailMoverEffect;
        

#if UNITY_EDITOR
        internal static VisualEffectResources GetOrCreateSettings()
        {
            var gameSettings    = GameSettingsObject.GetOrCreateSettings();
            var settings = gameSettings.GetOrCreate<VisualEffectResources>();
            
            return (VisualEffectResources)settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        
        public static VisualEffectResources GetSettings()
        {
            var gameSettings = GameSettingsObject.GetOrCreateSettings();
            var settings     = gameSettings.GetOrCreate<VisualEffectResources>();
            
            return (VisualEffectResources)settings;
        }
#endif
    }
}