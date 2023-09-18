using System.Collections.Generic;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using Collider = UnityEngine.Collider;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;
using MeshCollider = UnityEngine.MeshCollider;

namespace thelebaron.Destruction.Authoring
{
    public static class BaseMeshConversion
    {
        /// <summary> Fill out data format with all baking parameters </summary>
        public static void Intialize(GameObject gameObject, int seed, float density, int totalChunks, 
            Material insideMaterial, Material outsideMaterial, float breakForce)
        {
            var meshFilter   = gameObject.GetComponent<MeshFilter>();
            var meshInstance = Object.Instantiate(meshFilter.sharedMesh);
            
            //var path         = "Assets/Meshes/Fractured/" + meshFilter.sharedMesh.name + ".asset";
            // Save mesh as asset
            //AssetDatabase.CreateAsset(meshInstance, path);
            // Load asset as mesh
            //var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            
            var mesh = meshFilter.sharedMesh;
            var meshBounds = mesh.bounds;
            
            var bake = new BakeData
            {
                gameObject      = gameObject,
                name            = gameObject.name,
                seed            = new System.Random().Next(),
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
            
            ConvertMeshData(bake);

            gameObject.GetComponent<FractureAuthoring>().BakeData = bake;
        }
        
        /// <summary> Convert mesh into fractured format, using nvidia blast </summary>
        private static void ConvertMeshData(BakeData bake)
        {
            NvBlastExtUnity.setSeed(bake.seed);
            
            // Convert mesh into nvidia mesh format
            var nvMesh = NvFractureUtility.Mesh(bake);

            // Create fracture tool
            var fractureTool = NvFractureUtility.FractureTool(nvMesh);
            
            // Fracture mesh using voronoi algorithm
            NvFractureUtility.Voronoi(fractureTool, nvMesh, bake.totalChunks);

            // Finalize(unsure what it does)
            fractureTool.finalizeFracturing();

            // Pass the data on to make editor objects
            CreateAuthoringGameObjects(fractureTool, bake);
        }


        /// <summary> Create editor friendly gameobjects for the resulting baked data </summary>
        private static void CreateAuthoringGameObjects(NvFractureTool fractureTool, BakeData bake)
        {
            // Iterate through all generated chunks
            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                // Create a gameobject per chunk
                var chunk = new GameObject("Chunk_" + i);
                
                // Set it as a child under the original gameobject
                chunk.transform.SetParent(bake.gameObject.transform, false);

                AddUnityComponents(i, chunk, fractureTool, bake);
                
                AddDestructionComponents(chunk);
            }

            CreateNodeConnections(bake);
            Cleanup(bake);
        }
        
        
        private static void AddUnityComponents(int i, GameObject chunk, NvFractureTool fractureTool, BakeData bake)
        {
            var renderer = chunk.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                bake.outsideMaterial,
                bake.insideMaterial
            };

            var outside = fractureTool.getChunkMesh(i, false);
            var inside  = fractureTool.getChunkMesh(i, true);

            var mesh = outside.toUnityMesh();
            mesh.subMeshCount = 2;
            mesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);

            var meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            mesh.MarkDynamic();
            
            AssetDatabase.CreateAsset(mesh, "Assets/GeometryCollection/" + bake.name + "/" + "chunk_"+i+".mesh");
            
            var mc = chunk.AddComponent<MeshCollider>();
            //mc.inflateMesh = true;
            mc.convex = true;
            //if(mc.sharedMesh.triangles.Length>256)
            //Debug.Log("tri error? " + gameObject.name);
            
            var shapeAuthoring = chunk.AddComponent<PhysicsShapeAuthoring>();
            shapeAuthoring.SetConvexHull(ConvexHullGenerationParameters.Default );
            var bodyAuthoring = chunk.AddComponent<PhysicsBodyAuthoring>();
            bodyAuthoring.Mass = bake.totalMass / bake.totalChunks;
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
        private static void CreateNodeConnections(BakeData bake)
        {
            bake.nodes = bake.gameObject.GetComponentsInChildren<NodeAuthoring>();
            
            // Loop work for all nodes
            foreach (var node in bake.nodes)
            {
                var nodeVertices = node.Mesh.vertices;
                
                // Loop other nodes
                foreach (var otherNode in bake.nodes)
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
        
        
        private static void Cleanup(BakeData bake)
        {
            foreach (var node in bake.gameObject.GetComponentsInChildren<NodeAuthoring>())
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