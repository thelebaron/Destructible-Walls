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

        private FractureCache  cache;
        private Mesh                meshObject;
        
        private FractureWorkingData fractureWorkingData;
        
        public static void Open(FractureCache target)
        {
            FractureEditorWindow window = (FractureEditorWindow)GetWindow(typeof(FractureEditorWindow));
            window.cache = target;
            window.Show();
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
            
            GUILayout.Space(10);

            if (cache == null)
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
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        
                        cache = asset;
                        Selection.activeObject = cache;
                        
                    }
                }
                
            }
            else
            {
                GUILayout.Label("Fracture Cache:");
                cache = (FractureCache)EditorGUILayout.ObjectField(cache, typeof(FractureCache), true);
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
                    cache.Clear();
                    EditorFracturing.Intialize(cache, seed, density, totalChunks, outsideMaterial, insideMaterial, breakForce);
                    
                    AssetDatabase.Refresh();
                    // refresh inspector
                    EditorUtility.SetDirty(cache);
                }
            }

        }
    }
}