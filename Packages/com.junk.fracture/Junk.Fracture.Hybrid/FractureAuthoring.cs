﻿using System.Collections.Generic;
using System.Linq;
using Junk.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;

namespace Junk.Fracture.Hybrid
{
    [ExecuteAlways,SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class FractureAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("FractureNodeAsset")] public FractureCache FractureCache;
        
        public Material           insideMaterial;
        public Material           outsideMaterial;
        public float              breakForce = 100;

        public FractureWorkingData FractureWorkingData;
        
        private void Joints(GameObject child, float breakForce)
        {
            var rb = child.GetComponent<Rigidbody>();
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
            var overlaps = mesh.vertices
                .Select(v => child.transform.TransformPoint(v))
                .SelectMany(v => UnityEngine.Physics.OverlapSphere(v, 0.01f))
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
    }
    
    public struct FractureRoot : IComponentData
    {
        
    }

    public class FractureBaker : Baker<FractureAuthoring>
    {
        //
        public struct FractureChild : IBufferElementData
        {
            public Entity Child;
        }

        public struct Fracture : IComponentData
        {
            
        }
        
        public struct Fractured : IComponentData, IEnableableComponent
        {
            
        }
        
        [TemporaryBakingType]
        public class FractureRenderData : IComponentData
        {
            public Mesh     Mesh;
            public Material InsideMaterial;
            public Material OutsideMaterial;
            public ushort    SubMeshIndex;
            public int      MaterialIndex;
        }
        
        public override void Bake(FractureAuthoring authoring)
        {
            var transform        = LocalTransform.FromPositionRotationScale(authoring.transform.position, authoring.transform.rotation, authoring.transform.localScale.x);
            var entity           = GetEntity(TransformUsageFlags.Dynamic);
            var fractureChildren = AddBuffer<FractureChild>(entity);
            //AddBuffer<ConnectionGraph>(GetEntity(TransformUsageFlags.Dynamic));
            AddComponent<FractureRoot>(entity);
            AddComponent<Fractured>(entity);
            SetComponentEnabled<Fractured>(entity, false);
            //AddComponent(entity, LocalTransform.Identity);
            var assetData = authoring.FractureCache;

            var children = assetData.Children;
            BakeChildren(entity, transform, fractureChildren, children);
        }

        public void BakeChildren(Entity parent, LocalTransform tr, DynamicBuffer<FractureChild> children,
            List<FractureCache>     nodes)
        {
            foreach (var node in nodes)
            {
                var child = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, node.name);
                
                AddComponent(child, LocalTransform.Identity);
                AddComponent(child, new LocalToWorld{Value = float4x4.TRS(tr.Position, tr.Rotation, tr.Scale)});
                AddComponent<LocalTransform>(child);
                AddComponent<Fracture>(child);
                AddComponent<Fractured>(child);
                SetComponentEnabled<Fractured>(child, false);
                AddComponent<Prefab>(child);
                
                // Render Entities
                var submeshCount = node.Mesh.subMeshCount;
                if (submeshCount > 1)
                {
                    var linkedEntities = AddBuffer<LinkedEntityGroup>(child);
                    linkedEntities.Add(new LinkedEntityGroup {Value = child});
                    for (int i = 0; i < node.Mesh.subMeshCount; i++)
                    {
                        var renderEntity = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, node.name + " Render " + i);
                        AddComponent<Prefab>(renderEntity);
                        AddComponent(renderEntity, LocalTransform.Identity);
                        AddComponent(renderEntity, new LocalToWorld{Value = float4x4.identity});
                        AddComponentObject(renderEntity, new FractureRenderData
                        {
                            Mesh            = node.Mesh,
                            SubMeshIndex    = (ushort)i,
                            MaterialIndex   = i,
                            InsideMaterial  = node.InsideMaterial,
                            OutsideMaterial = node.OutsideMaterial
                        });
                        AddComponent(renderEntity, new Parent{ Value = child });
                        linkedEntities.Add(new LinkedEntityGroup {Value = renderEntity});
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
                
                
                children.Add(new FractureChild {Child = child});
                
                // Physics Entities
                var filter       = CollisionFilter.Default;
                var material     = Unity.Physics.Material.Default;
                
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
                
                if (node.Children.Count > 0)
                {
                    var childChildren = AddBuffer<FractureChild>(child);
                    BakeChildren(child, tr, childChildren, node.Children);
                }
            }
        }
    }
    
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct FractureBakingSystem : ISystem
    {
        private EntityQuery query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            //builder.WithAll<FractureBaker.FractureChild>();
            //builder.WithAll<FractureBaker.Fracture>();
            builder.WithAll<FractureBaker.FractureRenderData>();
            builder.WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab);
            query = builder.Build(ref state);
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.TempJob);
            
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var renderData = state.EntityManager.GetComponentObject<FractureBaker.FractureRenderData>(entity);
                // Rendering
                var desc             = new RenderMeshDescription(ShadowCastingMode.On, true, MotionVectorGenerationMode.Object, 0);
                var renderMeshArray  = new RenderMeshArray(new Material[2] { renderData.OutsideMaterial,renderData.InsideMaterial }, new Mesh[1]{ renderData.Mesh }); 
                var materialIndex    = renderData.MaterialIndex;
                var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(materialIndex, 0, renderData.SubMeshIndex);
                RenderMeshUtility.AddComponents(entity, state.EntityManager, desc, renderMeshArray, materialMeshInfo);
            }
            
            entities.Dispose();
        }
    }
    
    public partial struct TestFractureSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingletonRW<BeginInitializationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (fractureChildren, localToWorld, localTransform, entity) in SystemAPI.Query<DynamicBuffer<FractureBaker.FractureChild>, RefRO<LocalToWorld>, RefRO<LocalTransform>>().WithAll<FractureBaker.Fractured>().WithEntityAccess())
            {
                foreach (var fractureChild in fractureChildren)
                {
                    var child = fractureChild.Child;
                    
                    var fractureEntity = ecb.Instantiate(child);
                    // get scale from 4x4
                    var scale = localToWorld.ValueRO.Value.GetScale();
                    var tr = LocalTransform.FromPositionRotationScale(localTransform.ValueRO.Position, localTransform.ValueRO.Rotation, scale);
                    
                    ecb.SetComponent(fractureEntity, tr);
                    
                }
                ecb.DestroyEntity(entity);
            }
        }
    }
}