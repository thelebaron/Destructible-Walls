using Junk.Destroy.Baking;
using Junk.Destroy.Hybrid;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Junk.Destroy.Editor
{
    public class FractureCacheAssetImporter : AssetImporter
    {
        [OnOpenAsset(1)]
        public static bool OpenAsset(int instanceID, int line) 
        {
            FractureCache myScriptableObject = EditorUtility.InstanceIDToObject(instanceID) as FractureCache;
            if(myScriptableObject != null) 
            {
                FractureEditorWindow.Open(myScriptableObject);
                return true;
            }
            return false;
        }
    }
}