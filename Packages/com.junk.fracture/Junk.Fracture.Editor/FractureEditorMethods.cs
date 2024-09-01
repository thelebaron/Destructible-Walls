using System.Linq;
using Junk.Fracture.Hybrid;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Junk.Fracture.Editor
{
    internal static class FractureEditorMethods
    {
        internal static void Setup(FractureSetupData data, EditorMode mode)
        {
            string path;
            
            switch (mode)
            {
                case EditorMode.OpenEditor:
                {
                    if (data.target is DestructibleAuthoring authoring)
                    {
                        data.authoring = (DestructibleAuthoring)data.target;
                        data.cache     = authoring.Cache;
                    }

                    if (data.target is FractureCache cache)
                    {
                        data.cache           = (FractureCache)data.target;
                        data.insideMaterial  = cache.InsideMaterial;
                        data.outsideMaterial = cache.OutsideMaterial;
                    }

                    if (data.cache == null)
                    {
                        goto case EditorMode.CreateAssets;
                    }
                    
                    break;
                }
                case EditorMode.CreateAssets:
                {

                    var n    = AssetHandlingUtility.StripSpecialCharacters(data.target.name);
                    
                    if (data.authoring.GetComponent<MeshFilter>() == null)
                    {
                        var mesh = new Mesh();
                        // Note scopa does not use submeshes afaik so we do not worry about submeshes doing this
                        var meshFilters = data.authoring.GetComponentsInChildren<MeshFilter>();
                        if(data.authoring.GetComponentsInChildren<MeshFilter>().Length <1)
                            Debug.LogError("No meshfilters or child meshfilters found!");

                        var meshRenderers = data.authoring.GetComponentsInChildren<MeshRenderer>();
                        data.materialAssets = meshRenderers.Select(mr => mr.sharedMaterial).ToArray();

                        var combine = new CombineInstance[meshFilters.Length];

                        int i = 0;
                        while (i < meshFilters.Length)
                        {
                            combine[i].mesh      = meshFilters[i].sharedMesh;
                            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                            i++;
                        }
                        mesh.CombineMeshes(combine, true, false);

                        mesh.name = n;
                        //Debug.Log(n);
                        
                        // Create temporary gameobject for fbx export. This is necessary due to fbx not exporting submeshes unless there are assigned materials(plural). 
                        //  todo investigate nvidia fracture tool fails to fracture a mesh with multiple materials.
                        var temp = new GameObject();
                        temp.hideFlags                                    = HideFlags.HideAndDontSave;
                        temp.AddComponent<MeshFilter>().sharedMesh        = mesh;
                        temp.AddComponent<MeshRenderer>().sharedMaterial = data.materialAssets[0]; 
                        temp.name                                         = data.name;
                        
                        path = EditorUtility.SaveFilePanel("Save", "Assets/Models/CommonObjects", n, "fbx");
                        AssetHandlingUtility.ExportMesh(temp, path);
                        AssetDatabase.Refresh();

                        // Cleanup
                        //Object.DestroyImmediate(temp);

                        data.meshAsset               =  AssetHandlingUtility.LoadDefaultMesh(path);
                        
                        // Also initialise the materials for later use
                        data.outsideMaterial = data.materialAssets[0];
                        if(data.materialAssets.Length>1)
                            data.insideMaterial = data.materialAssets[1];
                    }
                    else
                    {
                        data.meshAsset = data.authoring.GetComponent<MeshFilter>().sharedMesh;
                        data.authoring.TryGetComponent(out MeshRenderer renderer);
                        if (renderer.sharedMaterials.Length > 1)
                        {
                            data.outsideMaterial = AssetHandlingUtility.GetMaterialAsset(renderer.sharedMaterials[0]);
                            data.insideMaterial  = AssetHandlingUtility.GetMaterialAsset(renderer.sharedMaterials[1]);
                            data.materialAssets  = AssetHandlingUtility.GetMaterialAssets(renderer.sharedMaterials);
                        }
                        else
                        {
                            data.outsideMaterial = AssetHandlingUtility.GetMaterialAsset(renderer.sharedMaterial);
                            data.insideMaterial  = AssetHandlingUtility.GetMaterialAsset(renderer.sharedMaterial);
                            data.materialAsset   = AssetHandlingUtility.GetMaterialAsset(renderer.sharedMaterial);
                        }
                    }
                    goto case EditorMode.CreateCache;
                }

                case EditorMode.CreateCache:
                    
                    // Open a dialog for saving a new asset
                    System.IO.Directory.CreateDirectory(UnityEngine.Application.dataPath + "/Content/FractureCache");

                    string name = data.target.name;
                    path = UnityEditor.EditorUtility.SaveFilePanel("Save", "Assets/Content/FractureCache", name, "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        // Create a new asset
                        var cacheAsset = ScriptableObject.CreateInstance<FractureCache>();
                        cacheAsset.StableHash = (ulong)name.GetHashCode() + (ulong)EditorApplication.timeSinceStartup;
                        cacheAsset.Root       = cacheAsset;
                        cacheAsset.Mesh       = data.meshAsset;

                        // Set defaults if we are quickly testing
                        if (data.insideMaterial == null)
                        {
                            // load from path: "packages/com.junk.fracture/Junk.Fracture/Materials/InsideMaterial.mat"
                            cacheAsset.InsideMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.junk.fracture/Junk.Fracture/Materials/InsideMaterial.mat");
                            data.insideMaterial  = cacheAsset.InsideMaterial;
                        }
                        else
                            cacheAsset.InsideMaterial = AssetHandlingUtility.GetMaterialAsset(data.insideMaterial);

                        if (data.outsideMaterial == null)
                        {
                            // load from path: "packages/com.junk.fracture/Junk.Fracture/Materials/OutsideMaterial.mat"
                            cacheAsset.OutsideMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.junk.fracture/Junk.Fracture/Materials/OutsideMaterial.mat");
                            data.outsideMaterial  = cacheAsset.OutsideMaterial;
                        }
                        else
                            cacheAsset.OutsideMaterial = AssetHandlingUtility.GetMaterialAsset(data.outsideMaterial);

                        data.labelText = path;

                        // Save the mesh asset to the specified path
                        DirectoriesUtility.Truncate(ref path);
                        AssetDatabase.CreateAsset(cacheAsset, path);

                        data.cache = cacheAsset;

                        // If we are working with a fracture authoring component, set the fracture node asset
                        if (data.authoring != null) 
                            data.authoring.Cache = cacheAsset;

                        if (data.authoring == null)
                        {
                            Selection.activeObject = cacheAsset;
                        }

                        // set dirty
                        AssetDatabase.SaveAssets();
                        EditorUtility.SetDirty(cacheAsset);
                        AssetDatabase.Refresh();
                    }

                    break;
                default:
                    break;
            }
        }
        
        public static GameObject CreatePrefab(DestructibleAuthoring component)
        {
            Material[] sharedMaterials = null;

            if (component.GetComponent<MeshRenderer>() == null)
                sharedMaterials = new Material[2] { component.Cache.InsideMaterial, component.Cache.OutsideMaterial };
            if(component.GetComponent<MeshRenderer>()!=null)
                sharedMaterials = component.GetComponent<MeshRenderer>().sharedMaterials;

            if (sharedMaterials == null)
            {
                Debug.LogError("Could not find shared materials for " + component.name);
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
            meshFilter.sharedMesh = component.Cache.Mesh;
            var meshRenderer = copy.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = sharedMaterials;
            
            // ecs
            var authoring = copy.AddComponent<FracturedAuthoring>();
            authoring.FractureCache = component.Cache;
            

            var path = UnityEditor.EditorUtility.SaveFilePanel("Save", "Assets/Prefabs/Destructibles", name, "prefab");
            path = AssetHandlingUtility.ConvertToRelativePath(path);
            PrefabUtility.SaveAsPrefabAsset(copy, path);
            Object.DestroyImmediate(copy);
            //var pPath      = "Assets/Prefabs/FracturedObjects/" + name + ".prefab";
            var prefabGuid = AssetDatabase.AssetPathToGUID(path);
            var assetPath  = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab     = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            AssetDatabase.Refresh();
            return prefab;
        }
    }
}