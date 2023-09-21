
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
            
            // draw label for total children on this node
            EditorGUILayout.LabelField("Total Children: " + GetTotalChildrenCount(nodeAsset));
            
            if(nodeAsset.Children.Count > 0)
            {
                // space
                GUILayout.Space(10);
                var list = nodeAsset.Children;
                // draw list
                for (var index = 0; index < list.Count; index++)
                {
                    var childAsset = list[index];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(childAsset, typeof(FractureNodeAsset), false);
                    // labelfield containing child count
                    EditorGUILayout.LabelField(" Children: " + GetTotalChildrenCount(childAsset));
                    // flexible space
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            
        }

        private static void DrawHierarchyTree(FractureNodeAsset nodeAsset)
        {
            EditorGUI.indentLevel++;
            HandleNode(nodeAsset);
            if (nodeAsset.Parent != null)
            {
                DrawHierarchyTree(nodeAsset.Parent);
            }
            EditorGUI.indentLevel--;
        }

        private static void HandleNode(FractureNodeAsset nodeAsset)
        {
            // Indent
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Node: " + nodeAsset.name);
            EditorGUILayout.ObjectField(nodeAsset, typeof(FractureNodeAsset), false);

            // Un-indent
            EditorGUI.indentLevel--;
        }
        public int GetTotalChildrenCount(FractureNodeAsset nodeAsset)
        {
            int count = 0;
            if (nodeAsset.Children.Count > 0)
            {
                count += nodeAsset.Children.Count;
                foreach (var child in nodeAsset.Children)
                {
                    count += GetTotalChildrenCount(child);
                }
            }
            return count;
        }
    }

}