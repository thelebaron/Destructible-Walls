using Junk.Destroy.Authoring;
using Junk.Destroy.Hybrid;

namespace Junk.Destroy.Baking
{
    using UnityEngine;
    using UnityEditor;

    public class FractureEditorWindow : EditorWindow
    {
        private string   labelText   = "";
        private float    density     = 500;
        private int      totalChunks = 20;
        private int      seed = -1;
        private Material insideMaterial;
        private Material outsideMaterial;
        private float    breakForce = 100;

        private FractureCache fractureCache;
        private FractureChild fractureChild;
        private Mesh          meshObject;
        private EditorMode    editorMode;
        
        private FractureWorkingData fractureWorkingData;
        
        public static void Open(ScriptableObject target)
        {
            if(target is FractureCache fractureCache)
            {
                var window = (FractureEditorWindow)GetWindow(typeof(FractureEditorWindow));
                window.Clear();
                window.editorMode = EditorMode.FractureWorkshop;
                window.fractureCache = fractureCache;
                window.Show();
            }
            else if (target is FractureChild fractureChild)
            {
                var window = (FractureEditorWindow)GetWindow(typeof(FractureEditorWindow));
                window.Clear();
                
                if(fractureChild.FractureCache != null)
                {
                    window.editorMode = EditorMode.FractureWorkshop;
                    window.fractureCache = fractureChild.FractureCache;
                    window.Show();
                    return;
                }
                window.editorMode = EditorMode.FractureChild;
                window.fractureChild = fractureChild;
                window.Show();
            }
        }

        private void Clear()
        {
            fractureCache = null;
            fractureChild = null;
        }
        
        [MenuItem("Tools/Fracture Editor")]
        public static void ShowWindow()
        {
            GetWindow<FractureEditorWindow>("Fracture Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Welcome !");
            labelText = Selection.activeObject == null ? labelText : Selection.activeObject.name;
            labelText = EditorGUILayout.TextField(labelText);
            // editorMode enum field grayed out
            EditorGUI.BeginDisabledGroup(true);
            editorMode = (EditorMode)EditorGUILayout.EnumPopup(editorMode);
            EditorGUI.EndDisabledGroup();
            // disable editorMode enum field
            //
            
            
            GUILayout.Space(10);

            if (fractureCache == null && editorMode == EditorMode.FractureCache)
            {
                GUILayout.Label("Target Mesh:");
                meshObject = (Mesh)EditorGUILayout.ObjectField(meshObject, typeof(Mesh), true);
                
                if (GUILayout.Button("New fracture"))
                {
                    // Open a dialog for saving a new mesh asset
                    //string savePath = EditorUtility.SaveFilePanel("Save", "Assets", "New Fracture Cache", "asset");
                    var name = meshObject.name;
                    var path = UnityEditor.EditorUtility.SaveFilePanel("Save", "Assets", name, "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        // Create a new asset
                        var asset = ScriptableObject.CreateInstance<FractureCache>();
                        asset.Mesh = meshObject;

                        labelText = path;
                        
                        // Save the mesh asset to the specified path
                        DirectoriesUtility.Truncate(ref path);
                        AssetDatabase.CreateAsset(asset, path);
                        
                        fractureCache          = asset;
                        Selection.activeObject = fractureCache;
                        
                        editorMode             = EditorMode.FractureWorkshop;
                        // set dirty
                        AssetDatabase.SaveAssets();
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.Refresh();
                    }
                }
                
            }
            if (editorMode == EditorMode.FractureChild)
            {
                /*if(fractureChild.FractureCache != null)
                {
                    editorMode = EditorMode.FractureWorkshop;
                    fractureCache = fractureChild.FractureCache;
                    return;
                }*/
                
                if (GUILayout.Button("New fracture cache"))
                {
                    var newCacheAsset = ScriptableObject.CreateInstance<FractureCache>();
                
                    newCacheAsset.name   = fractureChild.Shape.name + "_Cache";
                    newCacheAsset.Parent = fractureChild;
                
                    AssetDatabase.AddObjectToAsset(newCacheAsset, fractureChild);
                    
                    newCacheAsset.Mesh = fractureChild.Shape.GetMesh();
                    AssetDatabase.AddObjectToAsset(newCacheAsset.Mesh, fractureChild);
                
                    fractureChild.FractureCache = newCacheAsset;
                
                    Selection.activeObject = newCacheAsset;
                
                    FractureEditorWindow.Open(newCacheAsset);
                    AssetDatabase.Refresh();
                }
                
            }

            
            if(editorMode == EditorMode.FractureWorkshop)
            {
                GUILayout.Label("Fracture Cache:");
                fractureCache = (FractureCache)EditorGUILayout.ObjectField(fractureCache, typeof(FractureCache), true);
                GUILayout.Label("Density:");
                density = EditorGUILayout.FloatField(density);
                GUILayout.Label("Total Chunks:");
                totalChunks = EditorGUILayout.IntField(totalChunks);
                GUILayout.Label("Seed:");
                seed = EditorGUILayout.IntField(seed);
                GUILayout.Label("Inside Material:");
                insideMaterial = (Material)EditorGUILayout.ObjectField(insideMaterial, typeof(Material), true);
                GUILayout.Label("Outside Material:");
                outsideMaterial = (Material)EditorGUILayout.ObjectField(outsideMaterial, typeof(Material), true);
                //GUILayout.Label("Break Force:");
                //breakForce = EditorGUILayout.FloatField(breakForce);
                
                if (GUILayout.Button("Fracture it!"))
                {
                    if (seed == -1)
                        seed = new System.Random().Next();
                    fractureCache.Clear();
                    EditorFracturing.Intialize(fractureCache, seed, density, totalChunks, outsideMaterial, insideMaterial, breakForce);
                    
                    AssetDatabase.Refresh();
                    // refresh inspector
                    EditorUtility.SetDirty(fractureCache);
                }
            }
        }
        
        public enum EditorMode
        {
            FractureCache,
            FractureChild,
            FractureWorkshop
        }
    }
}