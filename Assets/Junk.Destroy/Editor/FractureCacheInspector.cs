
using UnityEngine;
using UnityEditor;
using Junk.Destroy.Hybrid;

namespace Junk.Destroy.Editor
{
    [UnityEditor.CustomEditor(typeof(FractureCache))]
    public class FractureCacheInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var fracture = target as FractureCache;


            if (GUILayout.Button("Up)"))
            {
                Selection.activeObject = fracture.Parent;
            }

            DrawDefaultInspector();
        }
        
    }

}