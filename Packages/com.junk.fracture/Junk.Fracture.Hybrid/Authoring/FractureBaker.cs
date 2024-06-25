using System.Collections.Generic;
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
            AddComponent<Fractured>(entity);
            SetComponentEnabled<Fractured>(entity, false);
            
            var fractureCache = authoring.FractureCache;
            var children      = fractureCache.Children;
            BakeChildren(entity, transform, graphBuffer, fractureChildren, children);
        }

        public void BakeChildren(Entity parent, LocalTransform tr, DynamicBuffer<FractureGraph> graph, DynamicBuffer<FractureChild> children, List<FractureCache> nodes)
        {
            Assert.IsTrue(nodes.Count > 0, "No children to bake");
            Assert.IsNotNull(nodes, "Nodes is null");
            
            foreach (var node in nodes)
            {
                var child = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, node.name);
                graph.Add(new FractureGraph {Node = child});
                AddComponent(child, LocalTransform.Identity);
                AddComponent(child, new LocalToWorld{Value = float4x4.TRS(tr.Position, tr.Rotation, tr.Scale)});
                AddComponent<LocalTransform>(child);
                AddComponent<Fractured>(child);
                SetComponentEnabled<Fractured>(child, false);
                AddComponent<Prefab>(child);
                AddComponent(child, new TimeDestroy{Value = 5});
                
                // Render Entities
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
                children.Add(new FractureChild {Child = child});

                // Physics Components Setup
                {
                    var filter   = CollisionFilter.Default;
                    var material = Unity.Physics.Material.Default;
                    
                    // create nativearrays of vertices and triangles
                    var vertices  = new NativeArray<float3>(node.Mesh.vertices.Length, Allocator.TempJob);
                    var triangles = new NativeArray<int>(node.Mesh.triangles.Length, Allocator.TempJob);
                    
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
                    var mesh = colliderBlob.Value.ToMesh();
                    mesh.hideFlags = HideFlags.None;
                    AddComponentObject(child, new ColliderMesh {Mesh = mesh});
                }
                
                if (node.Children.Count > 0)
                {
                    var childChildren = AddBuffer<FractureChild>(child);
                    BakeChildren(child, tr, graph, childChildren, node.Children);
                }
            }
        }
    }
}