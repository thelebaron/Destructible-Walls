
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

            if (fracture.Parent != null)
            {
                if (GUILayout.Button("^", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    Selection.activeObject = fracture.Parent;
                }
            }
            

            DrawDefaultInspector();
        }
        
    }

}