#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace CyberJunk.Procedural
{

 
    [CustomEditor(typeof(Generator))]
    public class GeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var generator = (Generator)target;
 
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Spawn", GUILayout.Height(40), GUILayout.Width(64)))
            {
                generator.Spawn();
            }
            if(GUILayout.Button("Clear", GUILayout.Height(40), GUILayout.Width(64)))
            {
                generator.Clear();
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif