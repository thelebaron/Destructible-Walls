using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


namespace Destructibles
{
    [CustomEditor(typeof(NodeAuthoring))]
    public class NodeAuthoringInspector : Editor
    {
        private void OnSceneGUI()
        {
            var node = target as NodeAuthoring;

            /*
            Handles.color = Color.magenta;
            Handles.color = Color.red;
            Handles.CircleHandleCap(0,fracture.transform.position + new Vector3(5, 0, 0),fracture.transform.rotation * Quaternion.LookRotation(new Vector3(1, 0, 0)),1,EventType.Repaint);


            //
            //text
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;

            Handles.BeginGUI();
            Vector3 pos = fracture.transform.position;
            Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
            GUI.Label(new Rect(pos2D.x, pos2D.y, 100, 100), pos.ToString(), style);
            Handles.EndGUI();
            //

            EditorGUI.BeginChangeCheck();
            
            for (int i = 0; i < fracture.m_Points.Count; i++)
            {
                Handles.color = Color.magenta;
                fracture.m_HandlePoints[i] = Handles.PositionHandle(fracture.m_HandlePoints[i], Quaternion.identity);
                fracture.Update();
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Look Target");
                //floor.lookTarget = lookTarget;
                
            }*/
        }

        public override void OnInspectorGUI()
        {
            var fracture = target as NodeAuthoring;

            if (GUILayout.Button("Toggle connections)"))
            {
                if (fracture != null)
                    fracture.ShowConnections = !fracture.ShowConnections; //Refresh in editor view
            }
            


            if (GUILayout.Button("Finalise"))
            {
                var root = fracture.transform.root;
                if (fracture.transform.parent == null)
                {
                    Debug.LogError("No fracturing root script, cannot continue.");
                    return;
                }

                var fractureTarget = root.transform.GetComponent<FractureAuthoring>();
                if (fractureTarget == null)
                {
                    Debug.LogError("No fracturing root script, cannot continue.");
                    return;
                }
                
                fractureTarget.FindAnchors();
                var nodechains = fractureTarget.GetComponentsInChildren<NodeChain>();
                foreach (var nodechain in nodechains)
                {
                    DestroyImmediate(nodechain);
                }
                
            }
            
            
            DrawDefaultInspector();

        }
        
    }
}