using System;
using Junk.Physics.Stateful;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using PhysMaterial = Unity.Physics.Material;

namespace Junk.Physics.Hybrid
{
    public static class PhysicsBakingUtility
    {
        /// <summary>
        /// Creates a trigger entity for the given entity. Note that transform will not be synced with the original entity.
        /// </summary>
        public static Entity CreateTriggerEntity(IBaker              baker,
                                                 Entity              entity,  
                                                 Component           authoring,
                                                 AuthoringPhysicsData authoringData,
                                                 CollisionFilter     collisionFilter,
                                                 string              name = "",
                                                 bool createAdditionalEntity = false)
        {
            if(createAdditionalEntity)
                 entity = baker.CreateAdditionalEntity(TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic, false, authoring.gameObject.name + name + " + Trigger");
            
            var material = PhysMaterial.Default;
            material.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
            
            var                                        physicsCollider    = new PhysicsCollider();
            BlobAssetReference<Unity.Physics.Collider> blobAssetReference = default;
            switch (authoringData.Geometry)
            {
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Box:
                    blobAssetReference = Unity.Physics.BoxCollider.Create(new BoxGeometry
                    {
                        BevelRadius = 0.05f,
                        Center      = authoringData.Offset,
                        Orientation = quaternion.identity,
                        Size        = authoringData.Size
                    }, collisionFilter, material);
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Sphere:
                    Debug.Log( "Sphere collider not implemented" );
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Capsule:
                    Debug.Log( "Capsule collider not implemented" );
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.ConvexHull:
                    Debug.Log( "ConvexHull collider not implemented" );
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Mesh:
                    Debug.Log( "Mesh collider not implemented" );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            physicsCollider.Value = blobAssetReference;
            

            baker.AddBlobAsset(ref physicsCollider.Value, out var blobhash);
            // Link this trigger to the weapon 
            baker.AddComponent(entity, physicsCollider);
            baker.AddSharedComponent(entity, new PhysicsWorldIndex());
            baker.AddComponent<StatefulTriggerEvent>(entity);
            
            return entity;
        }
        
        public static Entity CreateRigidbodyEntity(IBaker            baker,
                                                 Entity              entity,  
                                                 Component           authoring,
                                                 AuthoringPhysicsData authoringData,
                                                 CollisionFilter     collisionFilter,
                                                 string              name = "")
        {
            var rigidbodyEntity = baker.CreateAdditionalEntity(TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic, false, authoring.gameObject.name + name + " + Rigidbody");
            
            var material = PhysMaterial.Default;
            material.CollisionResponse = CollisionResponsePolicy.Collide;

            var                                        physicsCollider    = new PhysicsCollider();
            BlobAssetReference<Unity.Physics.Collider> blobAssetReference = default;
            switch (authoringData.Geometry)
            {
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Box:
                    blobAssetReference = Unity.Physics.BoxCollider.Create(new BoxGeometry
                    {
                        BevelRadius = 0.05f,
                        Center      = authoringData.Offset,
                        Orientation = quaternion.identity,
                        Size        = authoringData.Size
                    }, collisionFilter, material);
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Sphere:
                    Debug.Log( "Sphere collider not implemented" );
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Capsule:
                    Debug.Log( "Capsule collider not implemented" );
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.ConvexHull:
                    Debug.Log( "ConvexHull collider not implemented" );
                    break;
                case AuthoringPhysicsData.PhysicsAuthoringColliderType.Mesh:
                    Debug.Log( "Mesh collider not implemented" );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            physicsCollider.Value = blobAssetReference;
            

            baker.AddBlobAsset(ref physicsCollider.Value, out var blobhash);
            // Link this trigger to the weapon 
            baker.AddComponent(rigidbodyEntity, physicsCollider);
            
            
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

            baker.AddComponent(rigidbodyEntity, PhysicsMass.CreateDynamic(massProperties, authoringData.Mass));
            baker.AddComponent(rigidbodyEntity, new PhysicsVelocity());
            baker.AddSharedComponent(rigidbodyEntity, new PhysicsWorldIndex());
            
            return rigidbodyEntity;
        }
    }
}