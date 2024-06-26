using System;
using System.Collections.Generic;
using System.Linq;
using Junk.Collections;
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
    public class FractureBakingData 
    {
        public Entity     Entity;
        public int        Id;
        public Mesh       Mesh;
        public float3     Position;
        public quaternion Rotation;
        
        //public GameObject GameObject;
        
        // Filled out in GetOverlaps
        public DynamicBuffer<Connection> ConnectionsBuffer;
        
        public static implicit operator Entity(FractureBakingData e) => e.Entity;
        
        public bool Equals(int other)
        {
            return Id == other;
        }

        public int CompareTo(int other)
        {
            return Id.CompareTo(other);
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
            var allPaths = FindAllPaths(bakedFractureChildData);

            foreach (var child in bakedFractureChildData)
            {
                var childEntity = child.Entity;
                var childId     = child.Id;
                
                Dictionary<int, List<int>> kvp = allPaths[childId];
                
                
                AddComponent<Fracture>(childEntity, new Fracture
                {
                    Id = childEntity.Index,
                    ConnectionMap = BakeMappingToBlob(this, kvp)
                });
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
            data.Entity               = entity;
            data.Id                   = entity.Index;
            data.Mesh                 = mesh;
            data.Position             = tr.Position;
            data.Rotation             = tr.Rotation;
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
            foreach (var data in dataList)
            {
                var entity     = data.Entity;
                var mesh       = data.Mesh;
            
                var buffer = AddBuffer<Connection>(entity);
                data.ConnectionsBuffer = buffer;
                
                // get mesh data to calculate overlaps
                var vertices  = mesh.vertices;

                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex        = vertices[i];
                    var worldPosition = math.transform(float4x4.TRS(data.Position, data.Rotation, 1), vertex);

                    foreach (var other in dataList)
                    {
                        var otherEntity = other.Entity;
                        var otherMesh = other.Mesh;
                        foreach (var otherVertex in otherMesh.vertices)
                        {
                            var otherPosition = math.transform(float4x4.TRS(other.Position, other.Rotation, 1), otherVertex);
                            
                            if (!(Vector3.Distance(worldPosition, otherPosition) <= touchRadius)) 
                                continue;
                            
                            if (otherEntity != entity && !buffer.AsNativeArray().Contains(otherEntity))
                            {
                                buffer.Add(new Connection
                                {
                                    ConnectedEntity = otherEntity,
                                    ConnectedId = otherEntity.Index
                                });
                            }
                        }
                    }
                }
            }
        }
        
        public static Dictionary<int, Dictionary<int, List<int>>> FindAllPaths(List<FractureBakingData> dataList)
        {
            var shortestPaths = new Dictionary<int, Dictionary<int, List<int>>>();

            // Initialize shortest paths dictionary
            foreach (var data in dataList)
            {
                var id  = data.Id;
                var anchors = data.ConnectionsBuffer.AsNativeArray();
                
                // Initialize dictionary for entity
                shortestPaths[id] = new Dictionary<int, List<int>>();
                
                foreach (var other in dataList)
                {
                    var otherId = other.Id;
                    
                    if (id == otherId)
                    {
                        shortestPaths[id][otherId] = new List<int> { id };
                    }
                    else if (anchors.Any(anchor => anchor.ConnectedId == otherId))
                    {
                        shortestPaths[id][otherId] = new List<int> { id, otherId };
                    }
                    else
                    {
                        shortestPaths[id][otherId] = null;
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
                        if (shortestPaths[i.Id][k.Id] != null && shortestPaths[k.Id][j.Id] != null)
                        {
                            var newPath = new List<int>(shortestPaths[i.Id][k.Id]);
                            newPath.AddRange(shortestPaths[k.Id][j.Id].Skip(1));

                            if (shortestPaths[i.Id][j.Id] == null || newPath.Count < shortestPaths[i.Id][j.Id].Count)
                            {
                                shortestPaths[i.Id][j.Id] = newPath;
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
        
        /// <summary>
        /// What this does:
        /// Each fracture has all paths to each other fracture mapped out as an array of ids
        /// StartFracture --- [1, 2, 3, 4, 5] ---> EndFracture
        ///
        /// The fractures are stored as Ids, the entity indexes only for the path
        /// We build these into a blob, that stores a hashmap of the Ids to the array of Ids
        /// So for use, lookup the id of the fracture you want to find the path to, and you get an index which is used on the mappingIndex
        /// </summary>
        public static BlobAssetReference<FractureConnectionMapData> BakeMappingToBlob(IBaker baker, Dictionary<int, List<int>> dictionary)
        {
            var builder    = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<FractureConnectionMapData>();
    
            var mainBuilderArray = builder.Allocate(ref root.ConnectionMap, dictionary.Count);
            
            var hashMapBuilder = builder.AllocateHashMap(ref root.MappingIndex, dictionary.Count);
            for (int i = 0; i < dictionary.Count; i++)
            {
                var kvp       = dictionary.ElementAt(i);
                var keyId     = kvp.Key;
                
                hashMapBuilder.Add(keyId, i);
                
                var valueList = kvp.Value;
                if (kvp.Value == null)
                {
                    Debug.Log("No path found for fracture " + keyId);
                    continue;
                }
                var blobBuilderArray  = builder.Allocate(ref mainBuilderArray[i], valueList.Count);
                for (int j = 0; j < valueList.Count; j++)
                {
                    blobBuilderArray[j] = valueList[j];
                }
            }
        
            var blobReference = builder.CreateBlobAssetReference<FractureConnectionMapData>(Allocator.Persistent);
            baker.AddBlobAsset(ref blobReference, out var hash);
            builder.Dispose();
        
            return blobReference;
        }
    }
}