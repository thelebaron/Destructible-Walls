#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    // 
    public class GameSettingsObject : ScriptableObject
    {
        public const string k_GameSettingsPath = "Assets/Settings/GameSettings.asset";

#if UNITY_EDITOR
        [Range(0, 100)]
#endif
        [SerializeField] private float volume;
        [SerializeField] private int    number;
        [SerializeField] private string someString;
        [SerializeField] private GameObject prefab;
        
        public float Volume => volume;
        public int   Number => number;
        public string SomeString => someString;
        public GameObject Prefab => prefab;

        public List<ScriptableObject> subSettings = new List<ScriptableObject>();

#if UNITY_EDITOR
        public static GameSettingsObject GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettingsObject>(k_GameSettingsPath);
            if (settings == null)
            {
                CheckOrCreateFolders();
                settings              = ScriptableObject.CreateInstance<GameSettingsObject>();
                settings.volume     = 75;
                settings.number     = 42;
                settings.someString = "The answer to the universe";
                AssetDatabase.CreateAsset(settings, k_GameSettingsPath);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }
        
        public static void CheckOrCreateFolders()
        {
            var path = "Assets/Settings";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
        }
        
        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
        
        public ScriptableObject GetOrCreate<T>() where T : ScriptableObject
        {
            if(subSettings == null)
                subSettings = new List<ScriptableObject>();
            var found  = false;
            foreach (var subSetting in subSettings)
            {
                if (subSetting.GetType() == typeof(T))
                {
                    found = true;
                    return subSetting;
                }
            }
            
            var instance = ScriptableObject.CreateInstance<T>();
            instance.name = typeof(T).Name;
            subSettings.Add(instance);
                
            AssetDatabase.AddObjectToAsset(instance, this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
                
            return instance;
        }
#endif
    }
}