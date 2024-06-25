using System;
using System.Collections.Generic;
using System.Linq;
using Junk.Transforms;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using MeshCollider = Unity.Physics.MeshCollider;

// ReSharper disable Unity.Entities.MustBeSurroundedWithRefRwRo

namespace Junk.Fracture.Hybrid
{
    public struct EntityAnchor : IBufferElementData, IEquatable<Entity>, IComparable<Entity>
    {
        public Entity ConnectedEntity;

        public bool Equals(Entity other)
        {
            return ConnectedEntity == other;
        }

        public int CompareTo(Entity other)
        {
            return ConnectedEntity.Index.CompareTo(other.Index);
        }
        
        // Implicit operators
        public static implicit operator Entity(EntityAnchor e) => e.ConnectedEntity;
        public static implicit operator EntityAnchor(Entity e) => new EntityAnchor {ConnectedEntity = e};
    }
    
    //[DisableAutoCreation]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    unsafe public partial struct FractureNeighborBakingSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<FractureChild>();
            builder.WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab);
            query = builder.Build(ref state);
        }

        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb         = new EntityCommandBuffer(Allocator.Temp);
            var list        = new List<GameObject>();
            var dictionary = new Dictionary<Entity, GameObject>();
            
            foreach(var fractureChildren in SystemAPI.Query<DynamicBuffer<FractureChild>>()
                        .WithOptions(EntityQueryOptions.IncludePrefab))
            {
                var touchRadius = 0.01f;
                
                // Setup go proxies
                foreach (var fractureChild in fractureChildren)
                {
                    var entity         = fractureChild.Child;
                    var localTransform = SystemAPI.GetComponent<LocalTransform>(entity);
                    var colliderMesh   = state.EntityManager.GetComponentObject<ColliderMesh>(entity);
                    
                    var gameObject   = new GameObject("Fracture Child");
                    gameObject.transform.position = localTransform.Position;
                    gameObject.transform.rotation = localTransform.Rotation;
                    var meshCollider = gameObject.AddComponent<UnityEngine.MeshCollider>();
                    var rigidBody    = gameObject.AddComponent<UnityEngine.Rigidbody>();
                    rigidBody.mass = 1;
                    rigidBody.isKinematic = true;
                    meshCollider.sharedMesh = colliderMesh.Mesh;
                    var meshFilter = gameObject.AddComponent<UnityEngine.MeshFilter>();
                    meshFilter.sharedMesh = colliderMesh.Mesh;
                    var meshRenderer = gameObject.AddComponent<UnityEngine.MeshRenderer>();
                    
                    
                    meshRenderer.sharedMaterials = new[]
                    {
                        new UnityEngine.Material(Shader.Find("Universal Render Pipeline/Lit"))
                    };
                    
                    dictionary.Add(entity, gameObject);
                    list.Add(gameObject);
                }

                // Loop through all the fracture children and find overlaps
                foreach (var fractureChild in fractureChildren)
                {
                    var entity       = fractureChild.Child;
                    
                    var buffer       = ecb.AddBuffer<EntityAnchor>(entity);
                    // we 
                    var bufferList   = new List<Entity>();
                    var gameObject   = dictionary[entity];
                    var overlaps     = new HashSet<Rigidbody>();
                    var colliderMesh = state.EntityManager.GetComponentObject<ColliderMesh>(entity);
                    
                    GetOverlaps(ref state, entity, buffer, dictionary, list, gameObject, colliderMesh.Mesh, touchRadius);
                    
                    /*
                    for (var i = 0; i < colliderMesh.Mesh.vertices.Length; i++)
                    {
                        var vertex = colliderMesh.Mesh.vertices[i];
                        var worldPosition = gameObject.transform.TransformPoint(colliderMesh.Mesh.vertices[i]);
                        var hits          = UnityEngine.Physics.OverlapSphere(worldPosition, touchRadius);
                        Debug.Log("Hit: " + hits.Length);
                        for (var j = 0; j < hits.Length; j++)
                        {
                            //
                            if (list.Contains(hits[j].gameObject))
                            {
                                var otherEntity = lookupTable.First(kvp => kvp.Value == hits[j].gameObject).Key;
                                if(otherEntity != entity && !buffer.AsNativeArray().Contains(otherEntity))
                                    buffer.Add(new EntityAnchor {ConnectedEntity = otherEntity});
                                hits.Add(hits[j].GetComponent<Rigidbody>());
                            }
                        }
                    }
                    
                    foreach (var overlap in hits)
                    { 
                        if (overlap.gameObject != gameObject)
                        {
                            var  otherEntity = dictionary.First(kvp => kvp.Value == overlap.gameObject).Key;
                            //ecb.AppendToBuffer(otherEntity, new EntityAnchor {ConnectedEntity = entity});
                            
                            //var joint       = overlap.gameObject.AddComponent<FixedJoint>();
                            //joint.connectedBody = rb;
                            //joint.breakForce    = jointBreakForce;
                        }
                    }*/
                }

                // Cleanup after ourselves
                foreach (var go in dictionary.Values)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
                dictionary.Clear();
                list.Clear();
            }
            ecb.Playback(state.EntityManager);
        }

        void GetOverlaps(ref SystemState state,      Entity entity, DynamicBuffer<EntityAnchor> buffer, Dictionary<Entity, GameObject> dictionary, List<GameObject> list,
                                 GameObject      gameObject, Mesh   mesh,   float                       touchRadius = 0.01f)
        {
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
                        if (Vector3.Distance(worldPosition, otherPosition) <= touchRadius)
                        {
                            var otherEntity = dictionary.First(kvp => kvp.Value == other).Key;
                            if (otherEntity != entity && !buffer.AsNativeArray().Contains(otherEntity))
                            {
                                buffer.Add(new EntityAnchor { ConnectedEntity = otherEntity });
                            }
                            //overlaps.Add(potentialCollider.GetComponent<Rigidbody>());
                        }
                    }
                }
            }
        }
        
    }
}