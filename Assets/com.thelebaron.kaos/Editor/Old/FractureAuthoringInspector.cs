using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


namespace Destructibles
{
    [CustomEditor(typeof(FractureAuthoring))]
    public class FractureAuthoringInspector : Editor
    {
        private void OnSceneGUI()
        {
            var fracture = target as FractureAuthoring;

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
            var fracture = target as FractureAuthoring;
            
            if (GUILayout.Button("MUST CONTAIN MORE THAN 3 FRACTURES"))
            {
            }
            
            if (GUILayout.Button("Fracture mesh"))
            {
                if (fracture != null) 
                    fracture.Create(); //Refresh in editor view
            }
            /*
            if (GUILayout.Button("Cleanup"))
            {
                if (fracture != null) 
                    fracture.Cleanup();
            }*/
            //GUILayout.TextArea("Cleanup"))
            if (GUILayout.Button("Find Anchors"))
            {
                if (fracture != null) 
                    fracture.FindAnchors();
            }
            
            
            if (GUILayout.Button("Reset"))
            {
                if (fracture != null) 
                    fracture.Reset();
            }
            
            DrawDefaultInspector();

        }
        
    }
}