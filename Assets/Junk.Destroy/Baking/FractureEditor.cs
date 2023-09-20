using Junk.Destroy.Authoring;
using Junk.Destroy.Hybrid;
using UnityEngine;
using UnityEditor;

namespace Junk.Destroy.Baking
{

    public partial class FractureEditorWindow : EditorWindow
    {
        private string   labelText   = "";
        private float    density     = 500;
        private int      totalChunks = 20;
        private int      seed = -1;
        private Material insideMaterial;
        private Material outsideMaterial;
        private float    breakForce = 100;

        private FractureAuthoring fractureAuthoring;
        private FractureCache fractureCache;
        private FractureChild fractureChild;
        private Mesh          meshObject;
        private EditorMode    editorMode;
        
        private FractureWorkingData fractureWorkingData;

        private Object target;
        
        public static void Open(UnityEngine.Object target)
        {
            if(target is FractureAuthoring authoring)
            {
                var window = (FractureEditorWindow)GetWindow(typeof(FractureEditorWindow));
                window.Clear();
                window.editorMode    = EditorMode.FractureCache;
                window.fractureAuthoring = authoring;
                window.fractureCache = authoring.Cache;
                window.Show();
                return;
            }
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
                
                EditorGUI.indentLevel++;

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);  
                GUILayout.Label("Density:");
                density = EditorGUILayout.FloatField(density);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Total Chunks:");
                totalChunks = EditorGUILayout.IntField(totalChunks);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Seed:");
                seed = EditorGUILayout.IntField(seed);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Inside Material:");
                insideMaterial = (Material)EditorGUILayout.ObjectField(insideMaterial, typeof(Material), true);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label("Outside Material:");
                outsideMaterial = (Material)EditorGUILayout.ObjectField(outsideMaterial, typeof(Material), true);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                //GUILayout.Label("Break Force:");
                //breakForce = EditorGUILayout.FloatField(breakForce);
                
                EditorGUI.indentLevel++;

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
                
                
                EditorGUILayout.Space();

                // Render the preview scene into a texture and stick it
                // onto the current editor window. It'll behave like a custom game view.
                Rect rect = new Rect(0, 0, 512, 512);
                Debug.Log(previewUtility);
                previewUtility.BeginPreview(rect, previewBackground: GUIStyle.none);
                previewUtility.Render();
                var texture = previewUtility.EndPreview();
                GUI.DrawTexture(rect, texture);

                InitializePreview();
                DrawPreviewGui();
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }
        
        private void Update()
        {
            // Just do some random modifications here.
            //float time = (float)EditorApplication.timeSinceStartup * 15;
            //targetObject.transform.rotation = Quaternion.Euler(time * 2f, time * 4f, time * 3f);
		
            // Since this is the most important window in the editor, let's use our
            // resources to make this nice and smooth, even when running in the background.
            //Repaint();
        }
        
        public enum EditorMode
        {
            FractureCache,
            FractureChild,
            FractureWorkshop
        }
    }
}