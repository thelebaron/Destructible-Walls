using Junk.Fracture.Hybrid;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Junk.Fracture.Editor
{
    public class FractureNodeImporter : AssetImporter
    {
        [OnOpenAsset(1)]
        public static bool OpenAsset(int instanceID, int line) 
        {
            var target = EditorUtility.InstanceIDToObject(instanceID) as FractureCache;
            if(target != null) 
            {
                FractureEditor.Open(target);
                Selection.activeObject = target;
                return true;
            }
            return false;
        }
    }
}