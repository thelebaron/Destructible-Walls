﻿using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using thelebaron.Destruction.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Collider = UnityEngine.Collider;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;
using MeshCollider = UnityEngine.MeshCollider;

namespace thelebaron.Destruction
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [SelectionBase]
    public class FractureAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public  float       density     = 500;
        public  int         totalChunks = 20;
        public  int         seed;
        public  Mesh        mesh;
        public  Material    insideMaterial;
        public  Material    outsideMaterial;
        public  float       breakForce = 100;

        public BakeData BakeData;
        
        [Obsolete]
        private float       m_TotalMass;
        
        private Transform[] m_Children;

        private NodeAuthoring[] m_Nodes;
        public NodeAuthoring[] Nodes => m_Nodes;

        private const string MainPath = "Assets/GeometryCollection";
        
        private System.Random m_SystemRandom;
        
        
        
        // extract to separate class
        private void CreateMeshDirectories()
        {
            //if it doesn't, create it
            if(!Directory.Exists(MainPath))
                Directory.CreateDirectory(MainPath);
            
            var subPath = MainPath + "/" + name;
            //if it doesn't, create it
            if(!Directory.Exists(subPath))
                Directory.CreateDirectory(subPath);
        }
        
        [Obsolete]
        public void Create()
        {
            CreateMeshDirectories();
            m_SystemRandom = new System.Random();
            
            seed = m_SystemRandom.Next();
            m_TotalMass = density * (mesh.bounds.extents.x * mesh.bounds.extents.y * mesh.bounds.extents.z);
            Bake(this.gameObject);
        }


        private void Bake(GameObject go)
        {
            NvBlastExtUnity.setSeed(seed);

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

            Voronoi(fractureTool, nvMesh);

            fractureTool.finalizeFracturing();

            for (var i = 1; i < fractureTool.getChunkCount(); i++)
            {
                var chunk = new GameObject("Chunk_" + i);
                chunk.transform.SetParent(go.transform, false);

                Setup(i, chunk, fractureTool);
                
                AddAuthoringComponents(chunk,breakForce);
            }

            CreateNodeConnections();
            Cleanup();
        }

        private void AddAuthoringComponents(GameObject chunk, float breakForce)
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

        public void Cleanup()
        {
            foreach (var node in GetComponentsInChildren<NodeAuthoring>())
            {
                if (node.dirty)
                {
                    var rigidbodies = node.transform.GetComponents(typeof(Rigidbody));
                    var joints = node.transform.GetComponents(typeof(Joint));
                    var colliders = node.transform.GetComponents(typeof(MeshCollider));
                    
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
                        if(c is Collider)
                            DestroyImmediate(c);
                    }

                    node.dirty = false;
                }
            }
        }


        private void Setup(int i, GameObject chunk, NvFractureTool fractureTool)
        {
            var renderer = chunk.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                outsideMaterial,
                insideMaterial
            };

            var outside = fractureTool.getChunkMesh(i, false);
            var inside = fractureTool.getChunkMesh(i, true);

            var mesh = outside.toUnityMesh();
            mesh.subMeshCount = 2;
            mesh.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);

            var meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            mesh.MarkDynamic();
            
            AssetDatabase.CreateAsset(mesh, "Assets/GeometryCollection/" + name + "/" + "chunk_"+i+".mesh");
            
            //var rigid = chunk.AddComponent<Rigidbody>();
            //rigid.mass = m_TotalMass / totalChunks;

            var mc = chunk.AddComponent<MeshCollider>();
            //mc.inflateMesh = true;
            mc.convex = true;
            //if(mc.sharedMesh.triangles.Length>256)
                //Debug.Log("tri error? " + gameObject.name);
            
            var psa = chunk.AddComponent<PhysicsShapeAuthoring>();
            psa.SetConvexHull(ConvexHullGenerationParameters.Default );
            var pba = chunk.AddComponent<PhysicsBodyAuthoring>();
            pba.Mass = m_TotalMass / totalChunks;
        }


        private void CreateNodeConnections()
        {
            // Go through, sort nodes by distance for each node and add connections. Must be at least one 
            // on every connection and connections cannot be double the distance of the shortest distance.
            var nodes = GetComponentsInChildren<NodeAuthoring>();
            m_Nodes = nodes;
            var meshReferences = new List<MeshReference>();
            
            // Loop work for all nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                // Get current node
                var node = nodes[i];
                var nodeVertices = node.Mesh.vertices;
                
                // Loop other nodes
                foreach (var otherNode in nodes)
                {
                    if (otherNode.gameObject == node.gameObject)
                        continue;
                    //bool matchingConnection;
                    // other node's verts
                    var otherNodeVertices = otherNode.Mesh.vertices;
                    
                    //loop current nodes verts against other nodes verts
                    foreach (var vert in nodeVertices)
                    {
                        foreach (var othervert in otherNodeVertices)
                        {
                            // compare vertices with rounded decimals
                            if (MathUtility.SameVector(vert, othervert))
                            {
                                // if same position, add it
                                if (!node.connections.Contains(otherNode.transform))
                                {
                                    node.connections.Add(otherNode.transform);
                                    break;
                                }
                            }
                        }
                    }    

                }
            }
        }
        
        
        private void Joints(GameObject child, float breakForce)
        {
            var rb = child.GetComponent<Rigidbody>();
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
        
            
            
            var overlaps = mesh.vertices
                .Select(v => child.transform.TransformPoint(v))
                .SelectMany(v => Physics.OverlapSphere(v, 0.01f))
                .Where(o => o.GetComponent<Rigidbody>())
                .ToSet();

            foreach (var overlap in overlaps)
            { 
                if (overlap.gameObject != child.gameObject)
                {
                    var joint = overlap.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = rb;
                    joint.breakForce = breakForce;
                }
            }

            foreach (Transform tr in transform)
            {
                var connectednode = tr.gameObject.GetComponent<NodeAuthoring>();
                if (connectednode == null)
                {
                    var node = tr.gameObject.AddComponent<NodeAuthoring>();
                    node.dirty = true;
                }
                
                // Get all joints and add a node to each child with its joint neighbors
                var joints = tr.GetComponents<Joint>();
                foreach (var joint in joints)
                {
                    var node = joint.transform.GetComponent<NodeAuthoring>();
                    
                    if(!node.connections.Contains(joint.connectedBody.transform))
                        node.connections.Add(joint.connectedBody.transform);
                }
                
                var removeVelocity = tr.gameObject.GetComponent<RemoveVelocity>();
                if(removeVelocity==null)
                    tr.gameObject.AddComponent<RemoveVelocity>();
            }
            
        }


        public void Reset()
        {
            m_Children = GetComponentsInChildren<Transform>();
                
            for (int i = 0; i < m_Children.Length; i++)
            {
                if(i==0)
                    continue;
                DestroyImmediate(m_Children[i].gameObject);
            }

            m_Children = null;
        }
        
        // extract to separate class
        private void Voronoi(NvFractureTool fractureTool, NvMesh nvMesh)
        {
            var sites = new NvVoronoiSitesGenerator(nvMesh);
            sites.uniformlyGenerateSitesInMesh(totalChunks);
            fractureTool.voronoiFracturing(0, sites);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<ConnectionGraph>(entity);
            dstManager.SetName(entity, "Fracture Graph: "+ name);
        }

        public void FindAnchors()
        {
            // Then get all anchors, add to list and distribute to all nodes
            m_Nodes = GetComponentsInChildren<NodeAuthoring>();
            ConnectUnconnectedNodes();
            
            var anchorNodes = new List<Transform>();
            
            foreach (var node in m_Nodes)
            {
                //Reset any node anchor lists
                node.nodeLinks = new List<NestedNodeTrabsformList>();
                
                if(node.isAnchor && !anchorNodes.Contains(node.transform))
                    anchorNodes.Add(node.transform);
                
                
            }
            
            for (int i = 0; i < m_Nodes.Length; i++)
            {
                var node = m_Nodes[i];               
                node.anchors = anchorNodes;

                CreateAnchorConnectivityMap(node);
            }
        }

        /// <summary>
        /// Connects any nodes that didnt get connected initially
        /// </summary>
        private void ConnectUnconnectedNodes()
        {
            foreach (var node in m_Nodes)
            {
                if (node.connections.Count == 0)
                {
                    foreach (var subnode in m_Nodes)
                    {
                        if(subnode.connections.Contains(node.transform) && !node.connections.Contains(subnode.transform))
                            node.connections.Add(subnode.transform);
                    }
                }
            }
            
        }

        /// <summary>
        /// Bit of a recursive hell but: find all nodes connecting to an anchor
        /// </summary>
        private void CreateAnchorConnectivityMap(NodeAuthoring node)
        {
            foreach (var anchor in node.anchors)
            {
            
                var unused = new List<Transform>();

                Find(anchor, node,node, new List<Transform>(), 0);
            }
        }


        private void Find(Transform anchor, NodeAuthoring node, NodeAuthoring searchNode, List<Transform> list, int iterations)
        {
            const int max = 9999;
            iterations++;
            if (iterations >= max)
                return ;
            
            foreach (var connection in searchNode.connections)
            {
                if (connection == anchor)
                {
                    if (!list.Contains(connection))
                    {
                        list.Add(connection);
                        var chainAuthoring = node.gameObject.AddComponent<NodeChain>();
                        chainAuthoring.actuallyFoundAnchor = true;
                        chainAuthoring.AnchorList = list;
                        chainAuthoring.AnchorTransform = connection;
                        chainAuthoring.Nodes = m_Nodes;
                        chainAuthoring.Node = node;
                        chainAuthoring.m_Connections = node.connections;
                        chainAuthoring.ValidateList();
                    }
                    break;
                }

                if (!list.Contains(connection))
                {
                    list.Add(connection);
                    Find(anchor, node, connection.GetComponent<NodeAuthoring>(), list, iterations);
                }
            }
        }

        public void ClearChains()
        {
            
        }
    }

    
    public class MeshReference
    {
        public Mesh mesh;
        public GameObject meshObject;
    }
}