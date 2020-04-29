using System;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace kaos
{
    public static class Serialization
    {
        private static string DefaultPath()
        {
            var fileLocation = Application.dataPath; 
            var filename    = "KaosPreferences.json";
            var path = fileLocation + "\\" + filename;

            return path;
        }
        
        public static void Save(Preferences prefs)
        {
            if (prefs == null) 
                return;
            
            XmlSerializer serializer = new XmlSerializer(typeof(Preferences));
            TextWriter    textWriter = new StreamWriter(DefaultPath());
            serializer.Serialize(textWriter, prefs);
            textWriter.Close(); 
            var jsondata = JsonUtility.ToJson(prefs, true);
            File.WriteAllText(DefaultPath(), jsondata);
        }

        public static Preferences Load()
        {
            Preferences data = null;
            
            if (!File.Exists(DefaultPath()))
            {
                data = Preferences.Default();
            }
            
            if (File.Exists(DefaultPath()))
            {
                var x = File.ReadAllText(DefaultPath());
            
                data = (Preferences) JsonUtility.FromJson(x, typeof(Preferences));
            }

            return data;
        }

        public static string CreateDirectory(string path)
        {
            if (Directory.Exists(path))
                return path;
            
            Directory.CreateDirectory(path);
            return path;
        }
    }
  
    [Serializable]
    public class Preferences
    {
        public string Mesh;
        public string MaterialInside;
        public string MaterialOutside;
        public string MainDirectory;
        public string MeshDirectory;
        public string PrefabDirectory;

        public static Preferences Default()
        {
            var prefs = new Preferences();
            prefs.Mesh = null;
            prefs.MaterialOutside = "";
            prefs.MaterialInside = "";
            prefs.MainDirectory = "KaosData";
            prefs.MeshDirectory = "KaosData/"+"Mesh";
            prefs.PrefabDirectory = "KaosData/"+"Prefabs";
            return prefs;
        }
    }
}