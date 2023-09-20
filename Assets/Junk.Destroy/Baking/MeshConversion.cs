using System.Collections.Generic;
using Junk.Core.Creation;
using Junk.Destroy.Hybrid;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using Collider = UnityEngine.Collider;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;
using MeshCollider = UnityEngine.MeshCollider;

namespace Junk.Destroy.Authoring
{
    public static class EditorFracturing
    {
        /// <summary> Fill out data format with all baking parameters </summary>
        public static void Intialize(FractureNodeAsset cache, GameObject gameObject, int seed,
            float                                       density,            int totalChunks,
            Material                                    insideMaterial,     Material outsideMaterial, float breakForce)
        {
            var meshFilter   = gameObject.GetComponent<MeshFilter>();
            var meshInstance = Object.Instantiate(meshFilter.sharedMesh);
            
            var mesh = meshFilter.sharedMesh;
            var meshBounds = mesh.bounds;
            
            // We create a working data type and store its data and pass it through the helper methods
            var workingData = new FractureWorkingData
            {
                RootNodeAsset      = cache,
                gameObject      = gameObject,
                name            = gameObject.name,
                seed            = seed,
                random          = new System.Random(),
                density         = density,
                totalChunks     = totalChunks,
                mesh            = mesh,
                insideMaterial  = insideMaterial,
                outsideMaterial = outsideMaterial,
                jointBreakForce = breakForce,
                totalMass       = density * (meshBounds.extents.x * meshBounds.extents.y * meshBounds.extents.z)
            };
            DirectoriesUtility.CreateMeshDirectories(meshFilter.sharedMesh.name);
            AssetDatabase.CreateAsset(meshInstance, DirectoriesUtility.MainPath +"/"+ meshFilter.sharedMesh.name+"/"+ meshFilter.sharedMesh.name + ".asset");
            
            nvBlastCreateFractures(workingData);

            gameObject.GetComponent<FractureAuthoring>().FractureWorkingData = workingData;
        }
        
        public static void Intialize(FractureNodeAsset nodeAsset, int seed, float density, int totalChunks, Material inside, Material   outside, float breakForce)
        {
            var mesh         = nodeAsset.Mesh;
            var meshBounds = mesh.bounds;
            
            // We create a working data type and store its data and pass it through the helper methods
            var workingData = new FractureWorkingData
            {
                RootNodeAsset           = nodeAsset,
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
            //CreateAuthoringGameObjects(fractureTool, workingData);
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
        
        /// <summary> Create editor friendly gameobjects for the resulting baked data </summary>
        private static void CreateAuthoringGameObjects(NvFractureTool fractureTool, FractureWorkingData fractureWorking)
        {
            // Iterate through all generated chunks
            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                // Create a gameobject per chunk
                var chunk = new GameObject(fractureWorking.mesh.name + "_Chunk_" + i);
                
                // Set it as a child under the original gameobject
                chunk.transform.SetParent(fractureWorking.gameObject.transform, false);
                
                AddUnityComponents(i, chunk, fractureTool, fractureWorking);
                AddDestructionComponents(chunk);
                
            }
            CreateNodeConnections(fractureWorking);
            Cleanup(fractureWorking);
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
            fractureWorking.RootNodeAsset.Add(mesh, insideMaterial, outsideMaterial);
            AssetDatabase.AddObjectToAsset(mesh, fractureWorking.RootNodeAsset);
            
            // Save cacheAsset
            EditorUtility.SetDirty(fractureWorking.RootNodeAsset);
            AssetDatabase.SaveAssets();
        }
        
        private static void AddUnityComponents(int i, GameObject chunk, NvFractureTool fractureTool, FractureWorkingData fractureWorking)
        {
            var renderer = chunk.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                fractureWorking.outsideMaterial,
                fractureWorking.insideMaterial
            };

            var outside = fractureTool.getChunkMesh(i, false);
            var inside  = fractureTool.getChunkMesh(i, true);

            var mesh = outside.toUnityMesh();
            mesh.subMeshCount = 2;
            mesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);

            var meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            mesh.MarkDynamic();
            
            var mc = chunk.AddComponent<MeshCollider>();
            //mc.inflateMesh = true;
            mc.convex = true;
            //if(mc.sharedMesh.triangles.Length>256)
            //Debug.Log("tri error? " + gameObject.name);
            
            var shapeAuthoring = chunk.AddComponent<PhysicsShapeAuthoring>();
            shapeAuthoring.SetConvexHull(ConvexHullGenerationParameters.Default );
            var bodyAuthoring = chunk.AddComponent<PhysicsBodyAuthoring>();
            bodyAuthoring.Mass = fractureWorking.totalMass / fractureWorking.totalChunks;
        }
        
        private static void AddDestructionComponents(GameObject chunk)
        {
            if (chunk.gameObject.GetComponent<NodeAuthoring>() == null)
            {
                var node = chunk.gameObject.AddComponent<NodeAuthoring>();
                node.dirty = true;
            }
            
            if (chunk.gameObject.GetComponent<RemoveVelocity>() == null)
            {
                var removeVelocity = chunk.gameObject.AddComponent<RemoveVelocity>();
                removeVelocity.hideFlags = HideFlags.HideInInspector;
            }
            
        }
        
        
        /// Go through, sort nodes by distance for each node and add connections. Must be at least one 
        /// on every connection and connections cannot be double the distance of the shortest distance.
        private static void CreateNodeConnections(FractureWorkingData fractureWorking)
        {
            fractureWorking.nodes = fractureWorking.gameObject.GetComponentsInChildren<NodeAuthoring>();
            
            // Loop work for all nodes
            foreach (var node in fractureWorking.nodes)
            {
                var nodeVertices = node.Mesh.vertices;
                
                // Loop other nodes
                foreach (var otherNode in fractureWorking.nodes)
                {
                    if (otherNode.gameObject == node.gameObject)
                        continue;
                    
                    
                    // other node's verts
                    var otherNodeVertices = otherNode.Mesh.vertices;
                    
                    //loop current nodes verts against other nodes verts
                    foreach (var vert in nodeVertices)
                    {
                        foreach (var othervert in otherNodeVertices)
                        {
                            // compare vertices with rounded decimals
                            if (!MathUtility.SameVector(vert, othervert)) 
                                continue;
                            
                            // if same position, add it
                            if (node.connections.Contains(otherNode.transform)) 
                                continue;
                            
                            node.connections.Add(otherNode.transform);
                            break;
                        }
                    }
                }
            }
        }
        
        
        private static void Cleanup(FractureWorkingData fractureWorking)
        {
            foreach (var node in fractureWorking.gameObject.GetComponentsInChildren<NodeAuthoring>())
            {
                if (!node.dirty) 
                    continue;
                
                var rigidbodies = node.transform.GetComponents(typeof(Rigidbody));
                var joints      = node.transform.GetComponents(typeof(Joint));
                var colliders   = node.transform.GetComponents(typeof(MeshCollider));
                    
                foreach (var j in joints)
                {
                    if(j is Joint)
                        UnityEngine.Object.DestroyImmediate(j);
                }
                foreach (var r in rigidbodies)
                {
                    if(r is Rigidbody)
                        UnityEngine.Object.DestroyImmediate(r);
                }
            
                foreach (var c in colliders)
                {
                    if(c is Collider)
                        UnityEngine.Object.DestroyImmediate(c);
                }

                node.dirty = false;
            }
        }

    }
    
}