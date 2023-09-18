using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using thelebaron.Destruction.Authoring;


namespace thelebaron.Destruction
{
    [UnityEditor.CustomEditor(typeof(NodeAuthoring))]
    public class NodeAuthoringInspector : Editor
    {
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
                
                AnchorConversion.FindAnchors(fractureTarget.BakeData);
                //fractureTarget.FindAnchors(fractureTarget.ba);
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