using System;
using System.Collections.Generic;
using System.Linq;
using Junk.Entities;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using Material = UnityEngine.Material;

namespace Junk.Fracture.Hybrid
{
    [TemporaryBakingType]
    public class FractureRenderData : IComponentData
    {
        public Mesh     Mesh;
        public Material InsideMaterial;
        public Material OutsideMaterial;
        public ushort   SubMeshIndex;
        public int      MaterialIndex;
    }
    
    //[TemporaryBakingType]
    public class ColliderMesh : IComponentData
    {
        public Mesh Mesh;
    }

    /// <summary>
    /// Helper to store child fracture data for calculating overlaps
    /// </summary>
    public class FractureBakingData : IEquatable<Entity>, IComparable<Entity>
    {
        public Entity     Entity;
        public int        Id;
        public Mesh       Mesh;
        public GameObject GameObject;
        
        // Filled out in GetOverlaps
        public DynamicBuffer<Connection> ConnectionsBuffer;
        
        public static implicit operator Entity(FractureBakingData e) => e.Entity;
        
        public bool Equals(Entity other)
        {
            return Entity == other;
        }

        public int CompareTo(Entity other)
        {
            return Entity.Index.CompareTo(other.Index);
        }
    }
    
    public class FractureBaker : Baker<FracturedAuthoring>
    {
        public override void Bake(FracturedAuthoring authoring)
        {
            // Do not bake if no fracture cache is exists
            if(authoring.FractureCache == null)
                return;
            
            var transform         = LocalTransform.FromPositionRotationScale(authoring.transform.position, authoring.transform.rotation, authoring.transform.localScale.x);
            var entity            = GetEntity(TransformUsageFlags.Dynamic);
            var fractureChildren  = AddBuffer<FractureChild>(entity);
            
            AddComponent<FractureRoot>(entity);
            var graphBuffer = AddBuffer<FractureGraph>(entity);
            AddComponent<IsFractured>(entity);
            SetComponentEnabled<IsFractured>(entity, false);
            
            var fractureCache = authoring.FractureCache;
            var cacheNodes      = fractureCache.Children;
            
            // Initialize an array to store baked entity and gameobject data - to use for overlap detection
            var bakedFractureChildData = new List<FractureBakingData>();
            BakeChildren(entity, transform, graphBuffer, fractureChildren, cacheNodes, bakedFractureChildData);
            
            // All overlap detection is done here, and added to an Anchor buffer
            GetOverlaps(bakedFractureChildData);
            
            // Calculate the shortest path between all fractures
            FindAllShortestPaths(bakedFractureChildData);
            
            // Cleanup and destroy all temp gameobjects used for overlap detection
            Cleanup(bakedFractureChildData);
        }

        private void Cleanup(List<FractureBakingData> bakedFractureChildData)
        {
            foreach (var data in bakedFractureChildData)
            {
                UnityEngine.Object.DestroyImmediate(data.GameObject);
            }
        }

        /// <summary>
        /// Bake each child of the fracture cache to an entity
        /// </summary>
        private void BakeChildren(Entity parent, LocalTransform tr, DynamicBuffer<FractureGraph> graph, DynamicBuffer<FractureChild> children, List<FractureCache> nodes, List<FractureBakingData> bakedDataList)
        {
            Assert.IsTrue(nodes.Count > 0, "No children to bake");
            Assert.IsNotNull(nodes, "Nodes is null");
            
            foreach (var node in nodes)
            {
                var child = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, node.name);
                var id    = child.Index;
                AddComponent<Fracture>(child, new Fracture { Id = id });
                
                graph.Add(new FractureGraph {Node = child, Id = id});
                AddComponent(child, LocalTransform.Identity);
                AddComponent(child, new LocalToWorld{Value = float4x4.TRS(tr.Position, tr.Rotation, tr.Scale)});
                AddComponent<LocalTransform>(child);
                AddComponent<IsFractured>(child);
                SetComponentEnabled<IsFractured>(child, false);
                AddComponent<Prefab>(child);
                AddComponent(child, new TimeDestroy{Value = 5});
                
                // Render Entities
                AddGraphicsComponents(node, child);
                children.Add(new FractureChild {Child = child});

                // Physics Components Setup
                AddPhysicsComponents(node, child, out var mesh);
                
                // Add each child to baked data list
                bakedDataList.Add(GetFractureBakingData(child, tr, mesh));
                
                if (node.Children.Count > 0)
                {
                    var childChildren = AddBuffer<FractureChild>(child);
                    BakeChildren(child, tr, graph, childChildren, node.Children, bakedDataList);
                }
            }
        }

        /// <summary>
        /// Store additional data for later calculation of neighbours
        /// </summary>
        private FractureBakingData GetFractureBakingData(Entity entity, LocalTransform tr, Mesh mesh)
        {
            var data = new FractureBakingData();
            data.Entity = entity;
            data.Id = entity.Index;
            data.Mesh = mesh;
            data.GameObject = UnityEngine.Object.Instantiate(new GameObject());
            
            data.GameObject.transform.position = tr.Position;
            data.GameObject.transform.rotation = tr.Rotation;
            var meshCollider = data.GameObject.AddComponent<UnityEngine.MeshCollider>();
            var rigidBody    = data.GameObject.AddComponent<UnityEngine.Rigidbody>();
            rigidBody.mass          = 1;
            rigidBody.isKinematic   = true;
            meshCollider.sharedMesh = mesh;
            var meshFilter = data.GameObject.AddComponent<UnityEngine.MeshFilter>();
            meshFilter.sharedMesh = mesh;
            var meshRenderer = data.GameObject.AddComponent<UnityEngine.MeshRenderer>();
            
            meshRenderer.sharedMaterials = new[]
            {
                new UnityEngine.Material(Shader.Find("Universal Render Pipeline/Lit"))
            };
            return data;
        }

        private void AddPhysicsComponents(FractureCache node, Entity child, out Mesh mesh)
        {
            var filter   = CollisionFilter.Default;
            var material = Unity.Physics.Material.Default;
                    
            // create nativearrays of vertices and triangles
            var vertices  = new NativeArray<float3>(node.Mesh.vertices.Length, Allocator.Temp);
            var triangles = new NativeArray<int>(node.Mesh.triangles.Length, Allocator.Temp);
                    
            // copy vertices and triangles to nativearrays
            for (var index = 0; index < node.Mesh.vertices.Length; index++)
                vertices[index] = node.Mesh.vertices[index];
            for (var index = 0; index < node.Mesh.triangles.Length; index++)
                triangles[index] = node.Mesh.triangles[index];

            //var colliderBlob = Unity.Physics.MeshCollider.Create(vertices, triangles), filter, material);
            var colliderBlob = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default, CollisionFilter.Default);
                    
            AddBlobAsset(ref colliderBlob, out var blobhash);
            AddComponent(child, new PhysicsCollider { Value = colliderBlob });
            var massdist = new MassDistribution
            {
                Transform     = new RigidTransform(quaternion.identity, float3.zero),
                InertiaTensor = new float3(2f / 5f)
            };
            var massProperties = new MassProperties
            {
                AngularExpansionFactor = 0,
                MassDistribution       = massdist,
                Volume                 = 1
            };

            AddComponent(child, PhysicsMass.CreateDynamic(colliderBlob.Value.MassProperties, 12));
            AddComponent(child, new PhysicsVelocity());
            AddComponent(child, new PhysicsDamping{Linear = 0.05f, Angular = 0.05f});;
            AddSharedComponent(child, new PhysicsWorldIndex());
                    
            // for generating anchor and joint data
            mesh = colliderBlob.Value.ToMesh();
        }

        private void AddGraphicsComponents(FractureCache node, Entity child)
        {
            var submeshCount = node.Mesh.subMeshCount;
            if (submeshCount > 1)
            {
                var linkedEntities = AddBuffer<LinkedEntityGroup>(child);
                linkedEntities.Add(new LinkedEntityGroup { Value = child });
                for (int i = 0; i < node.Mesh.subMeshCount; i++)
                {
                    var renderEntity = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, node.name + " Render " + i);
                    AddComponent<Prefab>(renderEntity);
                    AddComponent(renderEntity, LocalTransform.Identity);
                    AddComponent(renderEntity, new LocalToWorld { Value = float4x4.identity });
                    AddComponentObject(renderEntity, new FractureRenderData
                    {
                        Mesh            = node.Mesh,
                        SubMeshIndex    = (ushort)i,
                        MaterialIndex   = i,
                        InsideMaterial  = node.InsideMaterial,
                        OutsideMaterial = node.OutsideMaterial
                    });
                    AddComponent(renderEntity, new Parent { Value    = child });
                    linkedEntities.Add(new LinkedEntityGroup { Value = renderEntity });
                }
            }
            else
            {
                AddComponentObject(child, new FractureRenderData
                {
                    Mesh            = node.Mesh,
                    InsideMaterial  = node.InsideMaterial,
                    OutsideMaterial = node.OutsideMaterial
                });
            }
        }
        
        /// <summary>
        /// Detect overlaps between fracture children using mesh data
        /// </summary>
        private void GetOverlaps(List<FractureBakingData> dataList, float touchRadius = 0.01f)
        {
            var list = dataList.Select(data => data.GameObject).ToList();
            var dictionary      = dataList.ToDictionary(data => data.Entity, data => data.GameObject);
            
            foreach (var data in dataList)
            {
                var entity     = data.Entity;
                var gameObject = data.GameObject;
                var mesh       = data.Mesh;
            
                var buffer = AddBuffer<Connection>(entity);
                data.ConnectionsBuffer = buffer;
                
                // get mesh data to calculate overlaps
                var vertices  = mesh.vertices;
                var transform = gameObject.transform;

                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex        = vertices[i];
                    var worldPosition = transform.TransformPoint(vertex);

                    foreach (var other in list)
                    {
                        var otherMesh = other.GetComponent<MeshFilter>().sharedMesh;
                        foreach (var otherVertex in otherMesh.vertices)
                        {
                            var otherPosition = other.transform.TransformPoint(otherVertex);
                            
                            if (!(Vector3.Distance(worldPosition, otherPosition) <= touchRadius)) 
                                continue;
                            
                            var otherEntity = dictionary.First(kvp => kvp.Value == other).Key;
                            if (otherEntity != entity && !buffer.AsNativeArray().Contains(otherEntity))
                            {
                                buffer.Add(new Connection { ConnectedEntity = otherEntity });
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Finds all shortest paths between all fractures
        /// </summary>
        public static Dictionary<Entity, Dictionary<Entity, List<Entity>>> FindAllShortestPaths(List<FractureBakingData> dataList)
        {
            var shortestPaths = new Dictionary<Entity, Dictionary<Entity, List<Entity>>>();

            // Initialize shortest paths dictionary
            foreach (var data in dataList)
            {
                var entity  = data.Entity;
                var anchors = data.ConnectionsBuffer.AsNativeArray();
                
                // Initialize dictionary for entity
                shortestPaths[entity] = new Dictionary<Entity, List<Entity>>();
                
                foreach (var other in dataList)
                {
                    var otherEntity = other.Entity;
                    
                    if (entity == otherEntity)
                    {
                        shortestPaths[entity][otherEntity] = new List<Entity> { entity };
                    }
                    else if (anchors.Any(anchor => anchor.ConnectedEntity == otherEntity))
                    {
                        shortestPaths[entity][otherEntity] = new List<Entity> { entity, otherEntity };
                    }
                    else
                    {
                        shortestPaths[entity][otherEntity] = null;
                    }
                }
            }

            // Floyd-Warshall algorithm
            foreach (var k in dataList)
            {
                foreach (var i in dataList)
                {
                    foreach (var j in dataList)
                    {
                        if (shortestPaths[i][k] != null && shortestPaths[k][j] != null)
                        {
                            
                            var newPath = new List<Entity>(shortestPaths[i][k]);
                            newPath.AddRange(shortestPaths[k][j].Skip(1));

                            if (shortestPaths[i][j] == null || newPath.Count < shortestPaths[i][j].Count)
                            {
                                shortestPaths[i][j] = newPath;
                            }
                        }
                    }
                }
            }
            
            return shortestPaths;
        }

        [InternalBufferCapacity(10)]
        public struct FractureIdEntityPath : IBufferElementData
        {
            public int    Id;
            public Entity Entity;
        }
        
        [InternalBufferCapacity(12)]
        public struct FracturePath : IBufferElementData
        {
            public int    Id;
        }
    }
}