using System;
using Junk.Fracture.Hybrid;
using UnityEngine;
using UnityEditor;

namespace Junk.Fracture.Editor
{
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(DestructibleAuthoring))]
    public class BreakableInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var breakAuthoring = target as DestructibleAuthoring;
            
            var cacheIsNull  = breakAuthoring.Cache == null;
            // show warning if cache is null
            if (cacheIsNull)
            {
                EditorGUILayout.HelpBox("Fracture Cache is not set. Please bake the fracture cache.", MessageType.Warning);
            }
            
            if (GUILayout.Button("Open Fracture Editor"))
            {
                FractureEditor.Open(breakAuthoring);
            }
            if (breakAuthoring.Cache!=null && GUILayout.Button("Delete Cache"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(breakAuthoring.Cache));
                breakAuthoring.Cache = null;
            }
            
            if(GUILayout.Button("Bake"))
            {
                var prefab    = FractureEditorMethods.CreatePrefab(breakAuthoring);
                breakAuthoring.Prefab = prefab;
                EditorUtility.SetDirty(breakAuthoring);
            }
            DrawDefaultInspector();
        }
    }
}