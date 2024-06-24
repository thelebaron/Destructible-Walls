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
                breakAuthoring.FracturedObject = prefab;
                EditorUtility.SetDirty(breakAuthoring);
            }
            DrawDefaultInspector();
        }
    }
}