using System.Collections.Generic;
using System.Linq;
using Junk.Destroy.Authoring;
using Junk.Destroy.Hybrid;
using Junk.Math;
using Project.Scripts.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;

namespace Junk.Destroy
{
    [ExecuteAlways,SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class FractureAuthoring : MonoBehaviour
    {
        public FractureNodeAsset FractureNodeAsset;
        
        public float              density     = 500;
        public int                totalChunks = 20;
        public int                seed;
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
        
        [TemporaryBakingType]
        public class FractureRenderData : IComponentData
        {
            public Mesh Mesh;
            public Material InsideMaterial;
            public Material OutsideMaterial;
        }
        
        public override void Bake(FractureAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var fractureChildren = AddBuffer<FractureChild>(entity);
            //AddBuffer<ConnectionGraph>(GetEntity(TransformUsageFlags.Dynamic));
            AddComponent<FractureRoot>(entity);
            //AddComponent(entity, LocalTransform.Identity);
            var assetData = authoring.FractureNodeAsset;

            var children = assetData.Children;
            BakeChildren(entity, fractureChildren, children);
        }

        public void BakeChildren(Entity parent, DynamicBuffer<FractureChild> children, List<FractureNodeAsset> nodes)
        {
            foreach (var node in nodes)
            {
                var child = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, node.name);
                
                AddComponent(child, LocalTransform.Identity);
                AddComponent(child, new LocalToWorld{Value = float4x4.identity});
                AddComponent<LocalTransform>(child);
                AddComponent<Fracture>(child);
                AddComponent<Prefab>(child);
                AddComponentObject(child, new FractureRenderData
                {
                    Mesh            = node.Mesh,
                    InsideMaterial  = node.InsideMaterial,
                    OutsideMaterial = node.OutsideMaterial
                });
                
                children.Add(new FractureChild {Child = child});
                
                if (node.Children.Count > 0)
                {
                    var childChildren = AddBuffer<FractureChild>(child);
                    BakeChildren(child, childChildren, node.Children);
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
            builder.WithAll<FractureBaker.Fracture>();
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
                var desc            = new RenderMeshDescription(ShadowCastingMode.On, true, MotionVectorGenerationMode.Object, 0);
                var renderMeshArray = new RenderMeshArray(new Material[2] { renderData.InsideMaterial, renderData.OutsideMaterial }, new Mesh[1]{ renderData.Mesh }); 
                var materialMeshInfo = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);
                RenderMeshUtility.AddComponents(entity, state.EntityManager, desc, renderMeshArray, materialMeshInfo);
            }
            
            entities.Dispose();
        }
    }

    public struct Bullshit : IComponentData
    {
        
    }
    public partial struct TestFractureSystem : ISystem
    {
        private EntityQuery query;
        
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<FractureBaker.Fracture>();
            query = builder.Build(ref state);
        }
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(Keyboard.current == null)
                return;
            
            var spaceKey = Keyboard.current.spaceKey;
            if(!spaceKey.isPressed)
                return;

            var ecb = SystemAPI.GetSingletonRW<BeginInitializationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            //var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (fractureChildren, localTransform, entity) in SystemAPI.Query<DynamicBuffer<FractureBaker.FractureChild>, RefRO<LocalTransform>>().WithAll<FractureRoot>().WithEntityAccess())
            {
                foreach (var fractureChild in fractureChildren)
                {
                    var child = fractureChild.Child;
                    
                    var fractureEntity = ecb.Instantiate(child);
                    ecb.SetComponent(fractureEntity, LocalTransform.FromPositionRotation(localTransform.ValueRO.Position, localTransform.ValueRO.Rotation));
                    
                }
                ecb.DestroyEntity(entity);
            }
            //ecb.Playback(state.EntityManager);
            //ecb.Dispose();
        }
    }
}