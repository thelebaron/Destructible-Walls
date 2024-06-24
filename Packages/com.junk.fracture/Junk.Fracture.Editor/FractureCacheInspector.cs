using Junk.Fracture.Hybrid;
using UnityEngine;
using UnityEditor;

namespace Junk.Fracture.Editor
{
    [UnityEditor.CustomEditor(typeof(FractureCache))]
    public class FractureCacheInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var nodeAsset = target as FractureCache;

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
                    EditorGUILayout.ObjectField(childAsset, typeof(FractureCache), false);
                    // labelfield containing child count
                    EditorGUILayout.LabelField(" Children: " + GetTotalChildrenCount(childAsset));
                    // flexible space
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            
        }

        private static void DrawHierarchyTree(FractureCache cache)
        {
            EditorGUI.indentLevel++;
            HandleNode(cache);
            if (cache.Parent != null)
            {
                DrawHierarchyTree(cache.Parent);
            }
            EditorGUI.indentLevel--;
        }

        private static void HandleNode(FractureCache cache)
        {
            // Indent
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Node: " + cache.name);
            EditorGUILayout.ObjectField(cache, typeof(FractureCache), false);

            // Un-indent
            EditorGUI.indentLevel--;
        }
        public int GetTotalChildrenCount(FractureCache cache)
        {
            int count = 0;
            if (cache.Children.Count > 0)
            {
                count += cache.Children.Count;
                foreach (var child in cache.Children)
                {
                    count += GetTotalChildrenCount(child);
                }
            }
            return count;
        }
    }

}