using System;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace kaos
{
    public static class KaosSerialization
    {
        
        public const string SavedMeshDataPath = "GeometryCollection";
        
        private static string DefaultPath()
        {
            var fileLocation = Application.dataPath; 
            var filename    = "KaosPreferences.json";
            var path = fileLocation + "\\" + filename;

            return path;
        }
        
        public static void Save(KaosPreferences prefs)
        {
            if (prefs == null) 
                return;
            
            XmlSerializer serializer = new XmlSerializer(typeof(KaosPreferences));
            TextWriter    textWriter = new StreamWriter(DefaultPath());
            serializer.Serialize(textWriter, prefs);
            textWriter.Close(); 
            var jsondata = JsonUtility.ToJson(prefs, true);
            File.WriteAllText(DefaultPath(), jsondata);
        }

        public static KaosPreferences Load()
        {
            KaosPreferences data = null;
            
            if (!File.Exists(DefaultPath()))
            {
                data = new KaosPreferences();
            }
            
            if (File.Exists(DefaultPath()))
            {
                var x = File.ReadAllText(DefaultPath());
            
                data = (KaosPreferences) JsonUtility.FromJson(x, typeof(KaosPreferences));
            }

            return data;
        }
    }
  
    [Serializable]
    public class KaosPreferences
    {
        public string Mesh;
        public string MaterialInside;
        public string MaterialOutside;
    }
}