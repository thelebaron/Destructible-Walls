#if UNITY_EDITOR
using UnityEditor;
#endif
using Junk.Entities.Hybrid;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    // This asset is nested under the GameSettingsObject
    public class ContactsSettingsObject : ScriptableObject
    {
        [SerializeField] private GameObject bloodsplatTiny;
        [SerializeField] private GameObject bulletholeTiny;
        [SerializeField] private GameObject debrisTiny1; // old pebble
        
        // Public accessors for baker to use
        public GameObject BloodsplatTiny => bloodsplatTiny;
        public GameObject BulletholeTiny => bulletholeTiny;
        public GameObject DebrisTiny1 => debrisTiny1;

#if UNITY_EDITOR
        internal static ContactsSettingsObject GetOrCreateSettings()
        {
            var gameSettings    = GameSettingsObject.GetOrCreateSettings();
            var contactSettings = gameSettings.GetOrCreate<ContactsSettingsObject>();
            return (ContactsSettingsObject)contactSettings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}