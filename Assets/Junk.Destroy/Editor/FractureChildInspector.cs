
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Junk.Destroy;
using Junk.Destroy.Baking;
using Junk.Destroy.Hybrid;
using Object = UnityEngine.Object;

namespace Junk.Destroy.Editor
{
    [UnityEditor.CustomEditor(typeof(FractureChild))]
    public class FractureChildInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var fracture = target as FractureChild;


            if (GUILayout.Button("Up)"))
            {
                Selection.activeObject = fracture.Parent;
            }

            if (GUILayout.Button("Create new fracture cache from this fracture)"))
            {
                var newCacheAsset = ScriptableObject.CreateInstance<FractureCache>();
                
                newCacheAsset.name = fracture.name + "_Cache";
                newCacheAsset.Parent = fracture;
                
                //check if asset.Parent is persistent
                Debug.Log(AssetDatabase.GetAssetPath(fracture.Parent));
                
                AssetDatabase.AddObjectToAsset(newCacheAsset, fracture.Parent);
                
                
                newCacheAsset.Mesh = fracture.Shape.GetMesh();
                AssetDatabase.AddObjectToAsset(newCacheAsset.Mesh, fracture.Parent);
                
                fracture.FractureCache = newCacheAsset;
                
                Selection.activeObject = newCacheAsset;
                
                FractureEditorWindow.Open(newCacheAsset);
                AssetDatabase.Refresh();
            }
            
            DrawDefaultInspector();
            
            if(fracture.FractureCache!= null)
            {
                EditorGUILayout.ObjectField(fracture.FractureCache, typeof(Object), false);
                // space
                GUILayout.Space(10);
                var list = fracture.FractureCache.Fractures;
                // draw list
                for (var index = 0; index < list.Count; index++)
                {
                    var f = list[index];
                    EditorGUILayout.ObjectField(f, typeof(FractureChild), false);
                }
            }
            

        }
        
    }

}