using Junk.Fracture.Hybrid;
using UnityEngine;
using UnityEditor;
using UnityEngine.Windows;

namespace Junk.Fracture.Editor
{
    public partial class FractureEditorWindow : EditorWindow
    {
        private string   labelText   = "";
        
        // voronoi settings
        private float density         = 500;
        private int   totalChunks     = 20;
        
        // clustered settings
        public  int   clusters        = 5;
        public  int   sitesPerCluster = 5;
        public  float clusterRadius   = 1;
        
        // slicing settings
        private Vector3Int slices            = Vector3Int.one;
        private float      offset_variations = 0;
        private float      angle_variations  = 0;
        private float      amplitude         = 0;
        private float      frequency         = 1;
        private int        octaveNumber      = 1;
        private int        surfaceResolution = 2;
        
        public enum FractureType
        {
            Voronoi,
            Clustered,
            Slicing
        }
        
        private FractureType fractureType = FractureType.Voronoi;
        
        private int      seed = -1;
        private Material insideMaterial;
        private Material outsideMaterial;
        private float    breakForce = 100;

        private BreakableAuthoring breakableAuthoring;
        private FractureCache  cache;
        private Mesh          meshObject;
        private EditorMode    editorMode;
        
        private FractureWorkingData fractureWorkingData;

        private Object target;
        
        public static void Open(UnityEngine.Object target)
        {
            var window = (FractureEditorWindow)GetWindow(typeof(FractureEditorWindow));
            window.Clear();
            
            if(target is BreakableAuthoring authoring)
            {
                window.breakableAuthoring = authoring;
                window.cache = authoring.FractureCache;
            }
            if (target is FractureCache cache)
            {
                window.cache = cache;
                
                window.insideMaterial  = cache.InsideMaterial;
                window.outsideMaterial = cache.OutsideMaterial;
                window.Repaint();
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
            if(breakableAuthoring != null)
                mode = 1;
            if(cache != null)
                mode = 2;
            
            switch (mode)
            {
                case 1:
                    editorMode = EditorMode.SetupFracture;
                    meshObject = breakableAuthoring.GetComponent<MeshFilter>().sharedMesh;
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
            cache = null;
        }

        public static GameObject CreatePrefab(BreakableAuthoring component)
        {
            var originalFilter = component.GetComponent<MeshFilter>();
            var originalRenderer = component.GetComponent<MeshRenderer>();
            var materials = originalRenderer.sharedMaterials;
            
            if (originalFilter == null)
            {
                Debug.LogError("MeshFilter not found");
                return null;
            }
            if (materials == null)
            {
                Debug.LogError("Materials not found");
                return null;
            }
            var name = component.gameObject.name + "_Fractured";
            if (!Directory.Exists("Assets/Prefabs/FracturedObjects"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "FracturedObjects");
            
            
            var copy = new GameObject(name);
            //var copy   = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            copy.transform.position   = component.transform.position;
            copy.transform.rotation   = component.transform.rotation;
            copy.transform.localScale = component.transform.localScale;
            // standard components
            var meshFilter = copy.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = originalFilter.sharedMesh;
            var meshRenderer = copy.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            
            // ecs
            var authoring = copy.AddComponent<FracturedAuthoring>();
            authoring.FractureCache = component.FractureCache;
            
            PrefabUtility.SaveAsPrefabAsset(copy, "Assets/Prefabs/FracturedObjects/" + name + ".prefab");
            Object.DestroyImmediate(copy);

            var pPath = "Assets/Prefabs/FracturedObjects/" + name + ".prefab";
            var prefabGuid = AssetDatabase.AssetPathToGUID(pPath);
            var assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab    = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            return prefab;
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

            if (cache == null && editorMode == EditorMode.SetupFracture)
            {
                GUILayout.Label("Target Mesh:");
                meshObject = (Mesh)EditorGUILayout.ObjectField(meshObject, typeof(Mesh), true);
                
                if (GUILayout.Button("New fracture"))
                {
                    // Open a dialog for saving a new mesh asset
                    //string savePath = EditorUtility.SaveFilePanel("Save", "Assets", "New Fracture Cache", "asset");
                    // Make directories if they don't exist for Assets/Content/FractureCache
                    System.IO.Directory.CreateDirectory(UnityEngine.Application.dataPath + "/Content/FractureCache");
                    var name = meshObject.name;
                    var path = UnityEditor.EditorUtility.SaveFilePanel("Save", "Assets/Content/FractureCache", name, "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        // Create a new asset
                        var asset = ScriptableObject.CreateInstance<FractureCache>();
                        asset.StableHash = (ulong)name.GetHashCode() + (ulong)EditorApplication.timeSinceStartup;
                        asset.Root       = asset;
                        asset.Mesh       = meshObject;

                        // Set defaults if we are quickly testing
                        if (insideMaterial == null)
                        {
                            // load from path: "packages/com.junk.fracture/Junk.Fracture/Materials/InsideMaterial.mat"
                            asset.InsideMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.junk.fracture/Junk.Fracture/Materials/InsideMaterial.mat");
                            insideMaterial = asset.InsideMaterial;
                        }
                        else
                        {
                            asset.InsideMaterial = insideMaterial;
                        }
                        if (outsideMaterial == null)
                        {
                            // load from path: "packages/com.junk.fracture/Junk.Fracture/Materials/OutsideMaterial.mat"
                            asset.OutsideMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.junk.fracture/Junk.Fracture/Materials/OutsideMaterial.mat");
                            outsideMaterial = asset.OutsideMaterial;
                        }
                        else
                        {
                            asset.OutsideMaterial = outsideMaterial;
                        }
                        
                        
                        labelText = path;
                        
                        // Save the mesh asset to the specified path
                        DirectoriesUtility.Truncate(ref path);
                        AssetDatabase.CreateAsset(asset, path);
                        
                        cache           = asset;
                        
                        // If we are working with a fracture authoring component, set the fracture node asset
                        if(breakableAuthoring != null)
                            breakableAuthoring.FractureCache = asset;
                        editorMode                  = EditorMode.FractureWorkshop;

                        if (breakableAuthoring == null)
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
                cache = (FractureCache)EditorGUILayout.ObjectField(cache, typeof(FractureCache), true);
                
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
                
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                //fractureType
                fractureType = (FractureType)EditorGUILayout.EnumPopup(fractureType);
                // switch
                switch (fractureType)
                {
                    case FractureType.Voronoi:

                        
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
                        EditorGUI.indentLevel++;
                        GUILayout.Space(20);

                        if (GUILayout.Button("Fracture node"))
                        {
                            if (seed == -1)
                                seed = new System.Random().Next();
                            cache.Clear();
                            EditorFracturing.Intialize(cache, seed, density, totalChunks, outsideMaterial,
                                insideMaterial, breakForce);

                            AssetDatabase.Refresh();
                            // refresh inspector
                            EditorUtility.SetDirty(cache);
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        
                        break;
                    case FractureType.Clustered:
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label("Clusters:");
                        clusters = EditorGUILayout.IntField(clusters);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label("Sites Per Cluster:");
                        sitesPerCluster = EditorGUILayout.IntField(sitesPerCluster);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label("Cluster Radius:");
                        clusterRadius = EditorGUILayout.FloatField(clusterRadius);
                        break;
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                
                EditorGUI.indentLevel++;
                        
                if (cache.Children.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    GUILayout.Space(20);
                    if (GUILayout.Button("Fracture children"))
                    {
                        if (seed == -1)
                            seed = new System.Random().Next();
                        for (var index = 0; index < cache.Children.Count; index++)
                        {
                            var child = cache.Children[index];
                            child.Clear();
                            EditorFracturing.Intialize(child, seed, density, totalChunks, outsideMaterial,
                                insideMaterial, breakForce);
                        }

                        AssetDatabase.Refresh();
                        // refresh inspector
                        EditorUtility.SetDirty(cache);
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