using System.Linq;
using Project.Scripts.Utils;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Collider = UnityEngine.Collider;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;
using MeshCollider = UnityEngine.MeshCollider;

namespace Destructibles
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class FractureAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private float density = 500;
        [SerializeField] private int totalChunks = 20;
        [SerializeField] private int seed;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material insideMaterial;
        [SerializeField] private Material outsideMaterial;
        [SerializeField] private float jointBreakForce = 100;
        private float totalMass;
        private Transform[] allChildren;
        private System.Random rng;

        private const string MainPath = "Assets/GeometryCollection";

        public void Create()
        {
            rng = new System.Random();
            seed = rng.Next();
            totalMass = density * (mesh.bounds.extents.x * mesh.bounds.extents.y * mesh.bounds.extents.z);
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
                var chunk = new GameObject("Chunk" + i);
                chunk.transform.SetParent(go.transform, false);

                Setup(i, chunk, fractureTool);
                Joints(chunk, jointBreakForce);
                //
            }
        }

        public void Cleanup()
        {
            var rigidbodies = GetComponentsInChildren(typeof(Rigidbody));
            var joints = GetComponentsInChildren(typeof(Joint));
            var colliders = GetComponentsInChildren(typeof(MeshCollider));
            
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
            
            // create a folder for the asset
            //string guid0 = AssetDatabase.CreateFolder("Assets//", "GeometryCollection");
            //string path0 = AssetDatabase.GUIDToAssetPath(guid0);
            
            AssetDatabase.CreateAsset(mesh, "Assets/GeometryCollection/" + name + "/" + "chunk_"+i+".mesh");
            //AssetDatabase.CreateAsset(mesh, newFolderPath + "/" + "chunk_"+i+".mesh");
            
            var rigidbody = chunk.AddComponent<Rigidbody>();
            rigidbody.mass = totalMass / totalChunks;

            var mc = chunk.AddComponent<MeshCollider>();
            //mc.inflateMesh = true;
            mc.convex = true;
            
            var psa = chunk.AddComponent<PhysicsShapeAuthoring>();
            psa.SetConvexHull(ConvexHullGenerationParameters.Default );
            var pba = chunk.AddComponent<PhysicsBodyAuthoring>();
            pba.Mass = totalMass / totalChunks;
        }
        
        private void Joints(GameObject child, float breakForce)
        {
            var rb = child.GetComponent<Rigidbody>();
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
        
            var overlaps = mesh.vertices
                .Select(v => child.transform.TransformPoint(v))
                .SelectMany(v => Physics.OverlapSphere(v, .01f))
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
                if(connectednode==null)
                    tr.gameObject.AddComponent<NodeAuthoring>();
                
                var joints = tr.GetComponents<Joint>();
                foreach (var joint in joints)
                {
                    var node = joint.transform.GetComponent<NodeAuthoring>();
                    if(!node.Connections.Contains(joint.connectedBody.transform))
                        node.Connections.Add(joint.connectedBody.transform);
                }
                
                var removeVelocity = tr.gameObject.GetComponent<RemoveVelocity>();
                if(removeVelocity==null)
                    tr.gameObject.AddComponent<RemoveVelocity>();
            }
        }
        
        public void MakeFolders()
        {
            if(!Directory.Exists(MainPath))
            {    
                //if it doesn't, create it
                Directory.CreateDirectory(MainPath);
 
            }

            var subPath = MainPath + "/" + name;
            if(!Directory.Exists(subPath))
            {    
                //if it doesn't, create it
                Directory.CreateDirectory(subPath);
 
            }
        }

        public void Reset()
        {
            allChildren = GetComponentsInChildren<Transform>();
                
            for (int i = 0; i < allChildren.Length; i++)
            {
                if(i==0)
                    continue;
                DestroyImmediate(allChildren[i].gameObject);
            }

            allChildren = null;
        }
        
        private void Voronoi(NvFractureTool fractureTool, NvMesh nvMesh)
        {
            var sites = new NvVoronoiSitesGenerator(nvMesh);
            sites.uniformlyGenerateSitesInMesh(totalChunks);
            fractureTool.voronoiFracturing(0, sites);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<ConnectionGraph>(entity);
            dstManager.SetName(entity, name);
        }
    }
    
    
    
    
}