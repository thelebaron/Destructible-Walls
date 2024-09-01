using UnityEditor;
using UnityEngine;

namespace Junk.Fracture.Editor
{
    public static class LogAssetPath
    {
        [MenuItem("Junk/Fracture/Log")]
        public static void LogPath()
        {
            var selection = Selection.activeObject;
            Debug.Log(selection.name);
            Debug.Log(AssetDatabase.GetAssetPath(selection));
        }
    }
}