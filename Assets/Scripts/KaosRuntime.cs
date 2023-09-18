using thelebaron.Destruction;
using kaos;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Joint = UnityEngine.Joint;
using MeshCollider = UnityEngine.MeshCollider;

public partial class KaosEditor
{
    private void OnFracture()
    {
        var mesh = (Mesh)meshField.value;
        if (mesh == null)
        {
            Debug.LogError("Cannot work without a valid mesh! Try assigning one!");
            return;
        }
        GetOrCreateDirectories(mesh);
        totalMass = density.value * (mesh.bounds.extents.x * mesh.bounds.extents.y * mesh.bounds.extents.z);
        if (fractureObject == null)
        {
            fractureObject = new GameObject();
            fractureObject.name = "Fracturable_"+fractureObject.name;
            
            //if(fractureObject.GetComponent<ConvertToEntity>()==null)
                //fractureObject.AddComponent<ConvertToEntity>();
            //prefab.hideFlags = HideFlags.DontSaveInEditor;
            //prefabField.value = prefab;
        }
        Bake(mesh);
        
    }
    
    private void GetOrCreateDirectories(Mesh mesh)
    {
        //Debug.Log(Application.dataPath);
        //Debug.Log(KaosSerialization.SavedMeshDataPath);
        //Debug.Log( preferences.MainDirectory + " show main");
        var mainPath = Application.dataPath +"/"+ preferences.MainDirectory;
        //Debug.Log(mainPath + " show dirs");
        //if it doesn't, create it
        if(!Directory.Exists(mainPath))
            Directory.CreateDirectory(mainPath);
        

        //if it doesn't, create it
        var subPath = mainPath + "/" + mesh.name;
        Directory.CreateDirectory(subPath);
        //Debug.Log("subPath dir " + subPath);
        
        if(!Directory.Exists(subPath))
        { 
            //Directory.CreateDirectory(subPath);
 
        }
    }
    
    private void Bake(Mesh mesh)
    {
        NvBlastExtUnity.setSeed(seed.value);

        var nvMesh = new NvMesh(
            mesh.vertices,
            mesh.normals,
            mesh.uv,
            mesh.vertexCount,
            mesh.GetIndices(0),
            (int) mesh.GetIndexCount(0)
        );

        var fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(false);
        fractureTool.setSourceMesh(nvMesh);

        if(fractureType.value.Equals(FractureType.Voronoi))
            Utils.Voronoi(fractureTool, nvMesh, fractureCount.value);
        if(fractureType.value.Equals(FractureType.Clustered))
            Utils.Clustered(fractureTool, nvMesh, Clusters.value, SitesPerCluster.value, ClusterRadius.value);
        if(fractureType.value.Equals(FractureType.Slicing))
            Utils.Slicing(fractureTool, nvMesh, slices.value, slicesOffset.value, slicesAngle.value, slicesAmplitude.value, slicesFrequency.value, slicesOctave.value, slicesSurfaceResolution.value);

        fractureTool.finalizeFracturing();
        //Profiler.BeginSample("");
        for (var i = 1; i < fractureTool.getChunkCount(); i++)
        {
            var chunk = new GameObject("Chunk_" + i);
            chunk.transform.SetParent(fractureObject.transform, false);

            CreateSubMeshes(i, chunk, fractureTool);
            //
            AddAuthoringComponents(chunk);
        }
/*
        CreateNodeConnections();
        Cleanup();*/
    }

    // ReSharper disable once UnusedMember.Local
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <param name="chunk"></param>
    /// <param name="fractureTool"></param>
    private void CreateSubMeshes(int i, GameObject chunk, NvFractureTool fractureTool)
    {
        var renderer = chunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[]
        {
            (UnityEngine.Material)materialOutideField.value,
            (UnityEngine.Material)materialInsideField.value
        };

        var outside = fractureTool.getChunkMesh(i, false);
        var inside  = fractureTool.getChunkMesh(i, true);

        var mesh = outside.toUnityMesh();
        mesh.subMeshCount = 2;
        mesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);

        var meshFilter = chunk.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        mesh.MarkDynamic();
        mesh.UploadMeshData(false);

        var meshDirectory = "Assets/" + preferences.MeshDirectory +"/" + fractureObject.name + "/";
        var filename = "fracture_level_" + fractureNestingSlider.value + "_" + i + ".mesh";
        Serialization.CreateDirectory(meshDirectory);
        
        var combined = meshDirectory + filename;
        AssetDatabase.CreateAsset(mesh, combined);
        
        var mc = chunk.AddComponent<MeshCollider>();
        //mc.inflateMesh = true;
        mc.convex = true;
        //if(mc.sharedMesh.triangles.Length>256)
        //Debug.Log("tri error? " + gameObject.name);
            
        var psa = chunk.AddComponent<PhysicsShapeAuthoring>();
        psa.SetConvexHull(ConvexHullGenerationParameters.Default );
        var pba = chunk.AddComponent<PhysicsBodyAuthoring>();
        pba.Mass = totalMass / fractureCount.value;
    }
    
    private void AddAuthoringComponents(GameObject chunk, float breakForce = 0)
    {
        if(!postprocess.value)
            return;
        
        var connectednode = chunk.gameObject.GetComponent<NodeAuthoring>();
        if (connectednode == null)
        {
            var node = chunk.gameObject.AddComponent<NodeAuthoring>();
            node.dirty = true;
        }
        var removeVelocity = chunk.gameObject.GetComponent<RemoveVelocity>();
        if (removeVelocity == null)
        {
            
            removeVelocity = chunk.gameObject.AddComponent<RemoveVelocity>();
            //removeVelocity.gameObject.hideFlags = HideFlags.HideInInspector;
        }
                
        if(chunk.gameObject.GetComponent<MeshRenderer>()==null)
            chunk.gameObject.AddComponent<MeshRenderer>();
    }

    public void Cleanup()
    {
        foreach (var node in fractureObject.GetComponentsInChildren<NodeAuthoring>())
        {
            if (node.dirty)
            {
                var rigidbodies = node.transform.GetComponents(typeof(UnityEngine.Rigidbody));
                var joints      = node.transform.GetComponents(typeof(UnityEngine.Joint));
                var colliders   = node.transform.GetComponents(typeof(MeshCollider));
                    
                foreach (var j in joints)
                {
                    if(j is Joint)
                        DestroyImmediate(j);
                }
                foreach (var r in rigidbodies)
                {
                    if(r is Rigidbody)
                        DestroyImmediate(r);
                }
            
                foreach (var c in colliders)
                {
                    if(c is UnityEngine.Collider)
                        DestroyImmediate(c);
                }

                node.dirty = false;
            }
        }
    }
    
    private void OnRandomSeed()
    {
        systemRandom = new System.Random();
        seed.value   = systemRandom.Next();
    }
}
