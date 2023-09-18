#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Junk.Procedural;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Junk.Workflow
{
    [Serializable]
    public class ModelData
    {
        public                   string        name;
        [HideInInspector] public string        path;
        [HideInInspector] public string        modelPath;
        [HideInInspector] public string        texturePath;
        [HideInInspector] public string        materialPath;
        public                   AssetCategory assetCategory;
        public                   AssetType     assetType;
        public                   GameObject    model;
        public                   Texture2D     texture;
        public                   Material      material;
        public                   GameObject    prefab;
        public                   List<string>  tags;
        public                   string        guid;
        
        public enum AssetCategory
        {
            CommonObjects,
            Residential,
            Bar,
            Nature,
        }

        public enum AssetType
        {
            Static,
            Dynamic,
            DestructibleStatic,
            DestructibleDynamic
        }
        
        #if UNITY_EDITOR
        public string FullPath()
        {
            return Application.dataPath + "/Models" + "/" + assetCategory + "/" + name;
        }
        
        public string RelativePath()
        {
            return "Assets/Models" + "/" + assetCategory + "/"+ name;
        }



        public void CreateAsset()
        {
            GameAssetUtility.CreateDirectory(this);
            GameAssetUtility.MoveAsset(this);
            GameAssetUtility.CreatePrefab(this);
            GameAssetUtility.SaveJsonData(this);
            AssetDatabase.Refresh();
        }
        


        public string Guid()
        {
            var guid   = "123456";

            
            
            
            var dictionary = new Dictionary<string, string>();
            var assetsJson = new List<string>();
            GameAssetUtility.DirectorySearch(assetsJson, Application.dataPath + "/Models" + "/");
            
            foreach (var j in assetsJson)
            {
                //Debug.Log(a);
            }

            return guid;
        }
        #endif
    }



    public static class GameAssetUtility
    {
        
        
        public static void CreateDirectory(ModelData modelData)
        {
            AssetDatabase.Refresh();
            var p = modelData.FullPath();
            if (!Directory.Exists(p))
            {
                modelData.path = modelData.FullPath();
                Directory.CreateDirectory(p);
                Debug.Log("Made directory " + p + " " + Directory.Exists(p));
                AssetDatabase.Refresh();
            }
        }

        
        
        public static void MoveAsset(ModelData modelData)
        {
            var modelPath   = AssetDatabase.GetAssetPath(modelData.model);
            var texturePath = AssetDatabase.GetAssetPath(modelData.texture);
            var materialPath = AssetDatabase.GetAssetPath(modelData.material);

            AssetDatabase.MoveAsset(modelPath, modelData.RelativePath()  + "/" + modelData.name + Path.GetExtension(modelPath));
            AssetDatabase.MoveAsset(texturePath, modelData.RelativePath()  + "/" + modelData.name+"_Diffuse" + Path.GetExtension(texturePath));
            AssetDatabase.MoveAsset(materialPath, modelData.RelativePath()  + "/" + modelData.name+"_Material" + Path.GetExtension(materialPath));
            AssetDatabase.Refresh();

            modelData.modelPath   = AssetDatabase.GetAssetPath(modelData.model);
            modelData.texturePath = AssetDatabase.GetAssetPath(modelData.texture);
            modelData.materialPath = AssetDatabase.GetAssetPath(modelData.material);

        }

        public static void SaveJsonData(ModelData modelData)
        {
            var path    = modelData.FullPath() + "/"+modelData.name + ".json";
            
            using(var writer = new StreamWriter(path, false))
            {
                var jsonData = JsonUtility.ToJson(modelData, true);
                writer.Write(jsonData);
                writer.Close();
            }
        }
        
        public static void CreatePrefab(ModelData modelData)
        {
            // Instantiate model into scene
            var instance = Object.Instantiate(modelData.model);
            {
                // Get renderer
                var renderer = instance.GetComponent<Renderer>();
                var renderers = instance.GetComponentsInChildren<Renderer>();
                // Look for child
                if (renderer == null)
                    renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                // Set material
                if (renderer != null)
                {
                    renderer.sharedMaterial = modelData.material;
                }

                if (renderers!=null && renderers.Length > 0)
                {
                    foreach (var rend in renderers)
                    {
                        rend.sharedMaterial = modelData.material;
                    }
                }
               
            }
            
            // TODO use conditional to check if procgen package is installed instead of hard dependency.
            instance.gameObject.AddComponent<ProceduralEntityData>();


            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, modelData.path + "/" + modelData.name + ".prefab");
            modelData.prefab = prefab;
            AssetDatabase.Refresh();
            Object.DestroyImmediate(instance);
        }
        
        public static void ValidateAssetType()
        {
            
            
        }
        
        
        public static void RecursiveGet()
        {
            var assets = new List<string>();
            DirectorySearch(assets, Application.dataPath + "/Models" + "/");
            foreach (var a in assets)
            {
                Debug.Log(a);
            }
        }
        
        public static void DirectorySearch(List<string> assets, string dir)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(dir))
                {
                    foreach (string path in Directory.GetFiles(d))
                    {
                        
                        var ext = Path.GetExtension(path);
                        if(ext==".json")
                            assets.Add(path);
                        
                    }
                    DirectorySearch(assets, d);
                }
            }
            catch (System.Exception excpt)
            {
                Debug.Log(excpt.Message);
            }
        }

        
        
        public static string UniqueGuid()
        {
            var dictionary = new Dictionary<string, string>();
            var jsonPaths = new List<string>();
            DirectorySearch(jsonPaths, Application.dataPath + "/Models" + "/");
            
            // Iterate all current assets, add to dictionary
            // then loop until find guid that doesnt exist to prevent hash collisions.
            foreach (var path in jsonPaths)
            {
                {
                    //var g = GenerateGuidString();
                    //Debug.Log(g);
                }
                var jsonString = File.ReadAllText(path);
                //var textAsset = //TextAsset;
                var modelData = JsonUtility.FromJson<ModelData>(jsonString);
                
                if(!dictionary.ContainsKey(modelData.guid))
                    dictionary.Add(modelData.guid, jsonString);
            }

            var guid = GenerateGuidString();
            
            while (true)
            {
                if(!dictionary.ContainsKey(guid))
                    break;
                guid = GenerateGuidString();
            }

            return guid;
        }


        private static string GenerateGuidString()
        {
            var g = new StringBuilder("123456789");
            
            for (int i = 0; i < 8; i++)
            {
                var randomSeed = UnityEngine.Random.Range(0, 12345678);
                g[i] = GetChar(randomSeed);
            }
            
            return g.ToString();
        }

        private static char GetChar(int seed)
        {
            //string        chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
            string        chars = "abcdefghijklmnopqrstuvwxyz1234567890";
            System.Random rand  = new System.Random(seed);
            int           num   = rand.Next(0, chars.Length -1);
            return chars[num];
        }
    }
}
#endif