
using System;
using kaos;
using UnityEngine;
using UnityEngine.Windows;

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
        
        Bake(mesh);
        
    }
    
    private static void GetOrCreateDirectories(Mesh mesh)
    {
        //Debug.Log(Application.dataPath);
        //Debug.Log(KaosSerialization.SavedMeshDataPath);
        var mainPath = Application.dataPath +"/"+ KaosSerialization.SavedMeshDataPath;
        if(!Directory.Exists(mainPath))
        {    
            //if it doesn't, create it
            Directory.CreateDirectory(mainPath);
        }
        //Debug.Log("mainPath dir " + mainPath);

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

        Utils.Voronoi(fractureTool, nvMesh, fractureCount.value);

        fractureTool.finalizeFracturing();
/*
        for (var i = 1; i < fractureTool.getChunkCount(); i++)
        {
            var chunk = new GameObject("Chunk_" + i);
            chunk.transform.SetParent(go.transform, false);

            Setup(i, chunk, fractureTool);
            //Joints(chunk, jointBreakForce);
            //
            AddAuthoringComponents(chunk,jointBreakForce);
            //SetupAuthoringComponents(chunk);
        }

        CreateNodeConnections();
        Cleanup();*/
    }

    
    private void OnRandomSeed()
    {
        systemRandom = new System.Random();
        seed.value   = systemRandom.Next();
    }
}
