using System;
using Junk.Fracture.Hybrid;
using UnityEngine;
using UnityEditor;

namespace Junk.Fracture.Editor
{
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(BreakableAuthoring))]
    public class BreakAuthoringInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var breakAuthoring = target as BreakableAuthoring;
            
            var cacheIsNull  = breakAuthoring.FractureCache == null;
            // show warning if cache is null
            if (cacheIsNull)
            {
                EditorGUILayout.HelpBox("Fracture Cache is not set. Please bake the fracture cache.", MessageType.Warning);
            }
            
            if (GUILayout.Button("Open Fracture Editor"))
            {
                FractureEditorWindow.Open(breakAuthoring);
            }
            if (breakAuthoring.FractureCache!=null && GUILayout.Button("Delete Cache"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(breakAuthoring.FractureCache));
                breakAuthoring.FractureCache = null;
            }
            
            if(GUILayout.Button("Bake"))
            {
                var prefab = FractureEditorWindow.CreatePrefab(breakAuthoring);
                breakAuthoring.FracturedPrefab = prefab;
                EditorUtility.SetDirty(breakAuthoring);
            }
            DrawDefaultInspector();
        }
    }
}