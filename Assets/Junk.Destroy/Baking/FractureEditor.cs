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
        private FractureNodeAsset  nodeAsset;
        private Mesh          meshObject;
        private EditorMode    editorMode;
        
        private FractureWorkingData fractureWorkingData;

        private Object target;
        
        public static void Open(UnityEngine.Object target)
        {
            var window = (FractureEditorWindow)GetWindow(typeof(FractureEditorWindow));
            window.Clear();
            
            if(target is FractureAuthoring authoring)
            {
                window.fractureAuthoring = authoring;
                window.nodeAsset = authoring.FractureNodeAsset;
            }
            if (target is FractureNodeAsset node)
            {
                window.nodeAsset = node;
            }
            window.Setup();
            window.Show();
        }
        
        [MenuItem("Tools/Fracture Editor")]
        public static void ShowWindow()
        {
            GetWindow<FractureEditorWindow>("Fracture Editor");
        }
        
        private void Setup()
        {
            int mode = 0;
            if(fractureAuthoring != null)
                mode = 1;
            if(nodeAsset != null)
                mode = 2;
            
            switch (mode)
            {
                case 1:
                    editorMode = EditorMode.SetupFracture;
                    meshObject = fractureAuthoring.GetComponent<MeshFilter>().sharedMesh;
                    insideMaterial = fractureAuthoring.insideMaterial;
                    outsideMaterial = fractureAuthoring.outsideMaterial;
                    break;
                case 2:
                    editorMode = EditorMode.FractureWorkshop;
                    break;
                default:
                    break;
            }
        }

        private void Clear()
        {
            nodeAsset = null;
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

            if (nodeAsset == null && editorMode == EditorMode.SetupFracture)
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
                        var asset = ScriptableObject.CreateInstance<FractureNodeAsset>();
                        asset.Root = asset;
                        asset.Mesh = meshObject;
                        asset.InsideMaterial = insideMaterial;
                        asset.OutsideMaterial = outsideMaterial;

                        labelText = path;
                        
                        // Save the mesh asset to the specified path
                        DirectoriesUtility.Truncate(ref path);
                        AssetDatabase.CreateAsset(asset, path);
                        
                        nodeAsset           = asset;
                        
                        // If we are working with a fracture authoring component, set the fracture node asset
                        if(fractureAuthoring != null)
                            fractureAuthoring.FractureNodeAsset = asset;
                        editorMode                  = EditorMode.FractureWorkshop;

                        if (fractureAuthoring == null)
                        {
                            Selection.activeObject = asset;
                            Setup();
                        }
                        
                        // set dirty
                        AssetDatabase.SaveAssets();
                        EditorUtility.SetDirty(asset);
                        AssetDatabase.Refresh();
                    }
                }
                
            }
            
            if(editorMode == EditorMode.FractureWorkshop)
            {
                GUILayout.Label("Fracture Cache:");
                nodeAsset = (FractureNodeAsset)EditorGUILayout.ObjectField(nodeAsset, typeof(FractureNodeAsset), true);
                
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
                
                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.Space(20);

                if (GUILayout.Button("Fracture node"))
                {
                    if (seed == -1)
                        seed = new System.Random().Next();
                    nodeAsset.Clear();
                    EditorFracturing.Intialize(nodeAsset, seed, density, totalChunks, outsideMaterial, insideMaterial, breakForce);
                    
                    AssetDatabase.Refresh();
                    // refresh inspector
                    EditorUtility.SetDirty(nodeAsset);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                 
                
                if (nodeAsset.Children.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    GUILayout.Space(20);
                    if (GUILayout.Button("Fracture children"))
                    {
                        if (seed == -1)
                            seed = new System.Random().Next();
                        foreach (var child in nodeAsset.Children)
                        {
                            child.Clear();
                            EditorFracturing.Intialize(child, seed, density, totalChunks, outsideMaterial, insideMaterial, breakForce);
                        }
                        AssetDatabase.Refresh();
                        // refresh inspector
                        EditorUtility.SetDirty(nodeAsset);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }
        
        private void Update()
        {
            // Just do some random modifications here.
            float time = (float)EditorApplication.timeSinceStartup * 15;
            //targetObject.transform.rotation = Quaternion.Euler(time * 2f, time * 4f, time * 3f);
            
            //if(fractureNodeAsset==null)
                //ShowWindow();
            
            
		
            // Since this is the most important window in the editor, let's use our
            // resources to make this nice and smooth, even when running in the background.
            Repaint();
        }
        
        public enum EditorMode
        {
            SetupFracture,
            FractureChild,
            FractureWorkshop
        }
    }
}