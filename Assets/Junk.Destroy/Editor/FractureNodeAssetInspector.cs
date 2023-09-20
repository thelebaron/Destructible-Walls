
using UnityEngine;
using UnityEditor;
using Junk.Destroy.Hybrid;

namespace Junk.Destroy.Editor
{
    [UnityEditor.CustomEditor(typeof(FractureNodeAsset))]
    public class FractureNodeAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var nodeAsset = target as FractureNodeAsset;

            if (nodeAsset.Parent != null)
            {
                // horizontal layout
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("^", GUILayout.Width(25), GUILayout.Height(25)))
                {
                    Selection.activeObject = nodeAsset.Parent;
                }
                EditorGUILayout.LabelField("Select Parent");
                EditorGUILayout.EndHorizontal();
            }

            if (nodeAsset.Parent!= null)
            {
                DrawHierarchyTree(nodeAsset);
            }
            else
            {
                EditorGUILayout.LabelField("Root Node");
            }

            GUILayout.Space(10);
            
            DrawDefaultInspector();
            
            if(nodeAsset.Children.Count > 0)
            {
                // space
                GUILayout.Space(10);
                var list = nodeAsset.Children;
                // draw list
                for (var index = 0; index < list.Count; index++)
                {
                    var childAsset = list[index];
                    EditorGUILayout.ObjectField(childAsset, typeof(FractureNodeAsset), false);
                }
            }
        }

        private static void DrawHierarchyTree(FractureNodeAsset nodeAsset)
        {
            // Indent
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Parent Node: " + nodeAsset.Parent.name);
            EditorGUILayout.ObjectField(nodeAsset.Parent, typeof(FractureNodeAsset), false);
            if(nodeAsset.Parent.Parent != null)
                DrawHierarchyTree(nodeAsset.Parent);
            
            EditorGUI.indentLevel--;
            
        }
    }

}