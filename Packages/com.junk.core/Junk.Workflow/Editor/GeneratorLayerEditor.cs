#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Junk.Workflow;
using UnityEditor;
using UnityEngine;
namespace CyberJunk.Procedural
{
    [CustomEditor(typeof(GeneratorLayer))]
    public class GeneratorLayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var generator = (GeneratorLayer)target;
 
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Get Prefab Data", GUILayout.Height(40), GUILayout.Width(264)))
            {
                generator.Refresh();
            }

            GUILayout.EndHorizontal();
        }
    }
}
#endif