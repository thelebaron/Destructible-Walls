using Nvidia;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using Material = UnityEngine.Material;
using MeshCollider = UnityEngine.MeshCollider;

namespace Junk.Fracture.Hybrid
{
    public static class EditorFracturing
    {
        public static void Intialize(FractureCache cache, int seed, float density, int totalChunks, Material inside, Material   outside, float breakForce)
        {
            var mesh         = cache.Mesh;
            var meshBounds = mesh.bounds;
            
            // We create a working data type and store its data and pass it through the helper methods
            var workingData = new FractureWorkingData
            {
                RootCache           = cache,
                seed            = seed,
                random          = new System.Random(),
                density         = density,
                totalChunks     = totalChunks,
                mesh            = mesh,
                insideMaterial  = inside,
                outsideMaterial = outside,
                jointBreakForce = breakForce,
                totalMass       = density * (meshBounds.extents.x * meshBounds.extents.y * meshBounds.extents.z)
            };
            
            nvBlastCreateFractures(workingData);
        }
        
        /// <summary> Convert mesh into fractured format, using nvidia blast </summary>
        // ReSharper disable once InconsistentNaming
        private static void nvBlastCreateFractures(FractureWorkingData workingData)
        {
            NvBlastExtUnity.setSeed(workingData.seed);
            
            // Convert mesh into nvidia mesh format
            var nvMesh = NvFractureUtility.Mesh(workingData);

            // Create fracture tool
            var fractureTool = NvFractureUtility.FractureTool(nvMesh);
            
            // Fracture mesh using voronoi algorithm
            NvFractureUtility.Voronoi(fractureTool, nvMesh, workingData.totalChunks);

            // Creates resulting fractured mesh geometry from intermediate format 
            fractureTool.finalizeFracturing();

            // Pass the data on to make editor objects
            CreateSubFractures(fractureTool, workingData);
        }

        /// <summary> Create editor friendly gameobjects for the resulting baked data </summary>
        private static void CreateSubFractures(NvFractureTool fractureTool, FractureWorkingData fractureWorking)
        {
            // Iterate through all generated chunks
            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                CreateFractureAsset(i, fractureTool, fractureWorking);
            }
        }
        
        private static void CreateFractureAsset(int index, NvFractureTool fractureTool, FractureWorkingData fractureWorking)
        {
            var insideMaterial  = fractureWorking.outsideMaterial;
            var outsideMaterial = fractureWorking.insideMaterial;
            var outsideChunkMesh         = fractureTool.getChunkMesh(index, false);
            var insideChunkMesh          = fractureTool.getChunkMesh(index, true);
            
            var mesh  = outsideChunkMesh.toUnityMesh();
            mesh.subMeshCount = 2;
            mesh.SetIndices(insideChunkMesh.getIndexes(), MeshTopology.Triangles, 1);
            mesh.name = fractureWorking.mesh.name + "_Chunk_" + index;
            fractureWorking.RootCache.Add(mesh, insideMaterial, outsideMaterial);
            AssetDatabase.AddObjectToAsset(mesh, fractureWorking.RootCache);
            
            // Save cacheAsset
            EditorUtility.SetDirty(fractureWorking.RootCache);
            AssetDatabase.SaveAssets();
        }
    }
}