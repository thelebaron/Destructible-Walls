using System;
using Junk.Fracture.Editor;
using Nvidia;
using UnityEditor;
using UnityEngine;
using Material = UnityEngine.Material;

namespace Junk.Fracture.Hybrid
{
    public static class NvidiaBridgeUtility
    {
        public static void Intialize(FractureCache cache, FractureSetupData data)
        {
            var mesh         = cache.Mesh;
            
            // We create a working data type and store its data and pass it through the helper methods
            var nvData = new NvFractureData
            {
                RootCache       = cache,
                seed            = data.seed,
                totalChunks     = data.totalChunks,
                mesh            = mesh,
                insideMaterial  = cache.InsideMaterial,
                outsideMaterial = cache.OutsideMaterial
            };

            NvBlastExtUnity.setSeed(nvData.seed);
            
            // Convert mesh into nvidia mesh format
            var nvMesh = NvFractureUtility.Mesh(nvData);

            // Create fracture tool
            var fractureTool = NvFractureUtility.FractureTool(nvMesh);

            switch (data.fractureType)
            {
                case FractureType.Voronoi:
                    NvFractureUtility.Voronoi(fractureTool, nvMesh, nvData.totalChunks);
                    break;
                case FractureType.Clustered:
                    NvFractureUtility.Clustered(fractureTool, nvMesh, data.clusters, data.sitesPerCluster, data.clusterRadius);
                    break;
                case FractureType.Slicing:
                    NvFractureUtility.Slicing(fractureTool, nvMesh, data.slices, data.offset_variations, data.angle_variations, data.amplitude, data.frequency, data.octaveNumber,
                        data.surfaceResolution);
                    break;
                default:
                    break;
            }


            // Creates resulting fractured mesh geometry from intermediate format 
            fractureTool.finalizeFracturing();

            // Pass the data on to make editor objects
            SaveAssets(fractureTool, nvData);
        }

        /// <summary> Create editor friendly gameobjects for the resulting baked data </summary>
        private static void SaveAssets(NvFractureTool fractureTool, NvFractureData nvFracture)
        {
            Debug.Log("CreateSubFractures called for " + fractureTool.getChunkCount());
            // Iterate through all generated chunks
            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                var insideMaterial   = nvFracture.outsideMaterial;
                var outsideMaterial  = nvFracture.insideMaterial;
                var outsideChunkMesh = fractureTool.getChunkMesh(i, false);
                var insideChunkMesh  = fractureTool.getChunkMesh(i, true);
            
                var mesh  = outsideChunkMesh.toUnityMesh();
                mesh.subMeshCount = 2;
                mesh.SetIndices(insideChunkMesh.getIndexes(), MeshTopology.Triangles, 1);
                mesh.name = nvFracture.mesh.name + "_Chunk_" + i;
                nvFracture.RootCache.Add(mesh, insideMaterial, outsideMaterial);
                AssetDatabase.AddObjectToAsset(mesh, nvFracture.RootCache);
            
                // Save cacheAsset
                EditorUtility.SetDirty(nvFracture.RootCache);
                AssetDatabase.SaveAssets();
            }
        }
    }
}