using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Junk.Break.Hybrid
{
    public static class DirectoriesUtility
    {
        /// <summary> Path to Geometry Collection </summary>
        public const string MainPath = "Assets/GeometryCollection";
        
        /// <summary> Create main directory if doesnt exist </summary>
        public static void CreateMeshDirectories(string name)
        {
            //if it doesn't, create it
            if(!Directory.Exists(MainPath))
                Directory.CreateDirectory(MainPath);
            
            var subPath = MainPath + "/" + name;
            //if it doesn't, create it
            if(!Directory.Exists(subPath))
                Directory.CreateDirectory(subPath);
        }
        
        /*public static void CreateAssetInFolder(Object newAsset, string parentFolder, string assetName )
        {
            string[] pathSegments            = parentFolder.Split( new char[] {'/'});
            string   accumulatedUnityFolder  = "Assets";
            string   accumulatedSystemFolder = Application.dataPath + System.IO.Path.GetDirectoryName("Assets" );
            foreach( string folder in pathSegments )
            {
                if (!System.IO.Directory.Exists( accumulatedSystemFolder + System.IO.Path.GetDirectoryName( accumulatedUnityFolder + "/" + folder )))
                    AssetDatabase.CreateFolder( accumulatedUnityFolder, folder );
                accumulatedSystemFolder += "/"+folder;
                accumulatedUnityFolder  += "/"+folder;
            }
        
            AssetDatabase.CreateAsset (newAsset, "Assets/"+parentFolder+"/"+assetName+".asset");
        }*/
        
        public static void CreateAssetInFolder(Object newAsset, string parentFolder, string assetName)
        {
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(string.Format("{0}/{1}", Application.dataPath, parentFolder));
            dirInfo.Create();
    
            AssetDatabase.CreateAsset(newAsset, string.Format("Assets/{0}/{1}.asset", parentFolder, assetName));
        }

        /// <summary>
        /// Removes all characters before Assets/ in a path
        /// </summary>
        /// <param name="path"></param>
        public static void Truncate(ref string path)
        {
            path = path.Substring(path.IndexOf("Assets/"));
        }
    }
}