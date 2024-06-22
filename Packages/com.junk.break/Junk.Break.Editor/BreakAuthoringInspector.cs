using System;
using Junk.Break.Hybrid;
using UnityEngine;
using UnityEditor;


namespace Junk.Break
{
    [CanEditMultipleObjects]
    [UnityEditor.CustomEditor(typeof(BreakableAuthoring))]
    public class BreakAuthoringInspector : UnityEditor.Editor
    {
        private SerializedProperty prefabProperty;
        
        private void OnSceneGUI()
        {
            var authoring = target as BreakableAuthoring;
        }

        private void OnEnable()
        {
            // Replace "prefabFieldName" with the name of your prefab field in the MonoBehaviour
            prefabProperty = serializedObject.FindProperty("FracturedObject");
        }

        public override void OnInspectorGUI()
        {
            var breakAuthoring = target as BreakableAuthoring;
            
            if (GUILayout.Button("Open Fracture Editor"))
            {
                FractureEditorWindow.Open(breakAuthoring);
            }
            if (breakAuthoring.FractureCache!=null && GUILayout.Button("Reset Cache"))
            {
                var cache = breakAuthoring.FractureCache;
                cache.Clear();
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