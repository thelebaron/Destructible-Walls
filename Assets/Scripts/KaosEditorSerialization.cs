using System;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace kaos
{
    public static class KaosEditorSerialization
    {
        private static string DefaultPath()
        {
            var fileLocation = Application.dataPath; 
            var filename    = "KaosData.json";
            var path = fileLocation + "\\" + filename;

            return path;
        }
        
        public static void Save(GameObject obj)
        {
            if (obj == null) return;
            
            var prefs = new KaosPreferences {
                SelectionName = obj.name
            };

            XmlSerializer serializer = new XmlSerializer(typeof(KaosPreferences));
            TextWriter    textWriter = new StreamWriter(DefaultPath());
            serializer.Serialize(textWriter, prefs);
            textWriter.Close(); 
            var jsondata = JsonUtility.ToJson(prefs, true);
            File.WriteAllText(DefaultPath(), jsondata);
        }

        public static KaosPreferences Load()
        {
            var x= File.ReadAllText(DefaultPath());
            
            var data = (KaosPreferences) JsonUtility.FromJson(x, typeof(KaosPreferences));
            
            return data;
        }
    }
  
    [Serializable]
    public class KaosPreferences
    {
        public string SelectionName;
    }
}