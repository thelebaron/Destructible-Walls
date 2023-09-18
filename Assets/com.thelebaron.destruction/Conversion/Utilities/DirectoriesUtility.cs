using UnityEngine.Windows;

namespace thelebaron.Destruction.Authoring
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
    }
}