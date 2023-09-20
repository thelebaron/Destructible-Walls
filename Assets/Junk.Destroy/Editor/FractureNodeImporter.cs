using Junk.Destroy.Baking;
using Junk.Destroy.Hybrid;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Junk.Destroy.Editor
{
    public class FractureNodeImporter : AssetImporter
    {
        [OnOpenAsset(1)]
        public static bool OpenAsset(int instanceID, int line) 
        {
            var target = EditorUtility.InstanceIDToObject(instanceID) as FractureNodeAsset;
            if(target != null) 
            {
                FractureEditorWindow.Open(target);
                Selection.activeObject = target;
                return true;
            }
            return false;
        }
    }
}