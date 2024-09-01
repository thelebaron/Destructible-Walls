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
        public enum PhysicsEntityFlags
        {
            SelfEntity, 
            NewEntity
        }
        public enum PhysicsRigidbodyFlags
        {
            Dynamic, 
            Kinematic
        }
        public struct TriggerData
        {
            // Worldspace
            public float3                       Center;
            public float3                       Extents;
            public CollisionFilter              Filter;
            public float                        Mass;
            public AuthoringPhysicsData.GeoType Geometry;
        }


        /// <summary>
        /// Creates a trigger entity for the given entity. Note that transform will not be synced with the original entity.
        ///
        /// 
        /// Note for future use when making triggers:
        /// For say doors, Layer 11 (environmental triggers).
        /// The belongs to should be set to 11, and collides with set to 3, for characters
        ///
        /// The player can collide with 11, and belongs to 3
        /// </summary>
        public static Entity CreateTriggerEntity(this IBaker baker,
               Entity entity, Component component, AuthoringPhysicsData authoringData, PhysicsEntityFlags flags)
        {
            if (flags == PhysicsEntityFlags.NewEntity)
                entity = baker.CreateAdditionalEntity(TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic, false, component.gameObject.name + " + Trigger");

            var material = PhysMaterial.Default;
            material.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

            var physicsCollider    = new PhysicsCollider();
            var blobAssetReference = default(BlobAssetReference<Unity.Physics.Collider>);
            
            switch (authoringData.Geometry)
            {
                case AuthoringPhysicsData.GeoType.Box:
                    blobAssetReference = Unity.Physics.BoxCollider.Create(new BoxGeometry
                    {
                        BevelRadius = 0.05f,
                        Center      = authoringData.Center,
                        Orientation = quaternion.identity,
                        Size        = authoringData.Extents
                    }, authoringData.Filter, material);
                    break;
                case AuthoringPhysicsData.GeoType.Sphere:
                    Debug.Log("Sphere collider not implemented");
                    break;
                case AuthoringPhysicsData.GeoType.Capsule:
                    Debug.Log("Capsule collider not implemented");
                    break;
                case AuthoringPhysicsData.GeoType.ConvexHull:
                    Debug.Log("ConvexHull collider not implemented");
                    break;
                case AuthoringPhysicsData.GeoType.Mesh:
                    Debug.Log("Mesh collider not implemented");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            physicsCollider.Value = blobAssetReference;
            baker.AddBlobAsset(ref physicsCollider.Value, out var blobhash);
            
            baker.AddComponent(entity, physicsCollider);
            baker.AddSharedComponent(entity, new PhysicsWorldIndex());
            
            if(flags == PhysicsBakingUtility.PhysicsEntityFlags.NewEntity)
            {
                baker.AddComponent(entity, LocalTransform.FromPositionRotation(component.transform.position, component.transform.rotation));
                baker.AddComponent(entity, new LocalToWorld { Value = float4x4.identity });
            }
            
            return entity;
        }

        public static Entity CreateStatefulTriggerEntity(IBaker baker,
               Entity entity, Component authoring, AuthoringPhysicsData authoringData, PhysicsEntityFlags flags)
        {
            var triggerEntity = CreateTriggerEntity(baker, entity, authoring, authoringData, flags);
            baker.AddComponent<StatefulTriggerEvent>(triggerEntity);
            return triggerEntity;
        }

        public static Entity CreateRigidbodyEntity(this IBaker baker,
               Entity entity, Component component, AuthoringPhysicsData authoringData, PhysicsEntityFlags entityFlags, PhysicsRigidbodyFlags rigidbodyFlags)
        {

            if (entityFlags == PhysicsBakingUtility.PhysicsEntityFlags.NewEntity)
            {
                entity = baker.CreateAdditionalEntity(TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic, false, component.gameObject.name + " + Rigidbody");
            }

            var material = PhysMaterial.Default;
            material.CollisionResponse = CollisionResponsePolicy.Collide;

            var physicsCollider    = new PhysicsCollider();
            var blobAssetReference = default(BlobAssetReference<Unity.Physics.Collider>);
            
            switch (authoringData.Geometry)
            {
                case AuthoringPhysicsData.GeoType.Box:
                    blobAssetReference = Unity.Physics.BoxCollider.Create(new BoxGeometry
                    {
                        BevelRadius = 0.05f,
                        Center      = authoringData.Center,
                        Orientation = quaternion.identity,
                        Size        = authoringData.Extents
                    }, authoringData.Filter, material);
                    break;
                case AuthoringPhysicsData.GeoType.Sphere:
                    Debug.Log("Sphere collider not implemented");
                    break;
                case AuthoringPhysicsData.GeoType.Capsule:
                    Debug.Log("Capsule collider not implemented");
                    break;
                case AuthoringPhysicsData.GeoType.ConvexHull:
                    Debug.Log("ConvexHull collider not implemented");
                    break;
                case AuthoringPhysicsData.GeoType.Mesh:
                    Debug.Log("Mesh collider not implemented");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            physicsCollider.Value = blobAssetReference;


            baker.AddBlobAsset(ref physicsCollider.Value, out var blobhash);
            // Link this trigger to the weapon 
            baker.AddComponent(entity, physicsCollider);


            var massdist = new MassDistribution
            {
                Transform     = new RigidTransform(quaternion.identity, float3.zero),
                InertiaTensor = new float3(2f / 5f)
            };

            /*switch (rigidbodyFlags)
            {
                case PhysicsRigidbodyFlags.Dynamic:
                    break;
                case PhysicsRigidbodyFlags.Kinematic:
                    break;
            }*/
            if (rigidbodyFlags == PhysicsRigidbodyFlags.Dynamic)
            {
                var massProperties = new MassProperties
                {
                    AngularExpansionFactor = 0,
                    MassDistribution       = massdist,
                    Volume                 = 1
                };
                baker.AddComponent(entity, PhysicsMass.CreateDynamic(massProperties, authoringData.Mass));
            }
            
            if (rigidbodyFlags == PhysicsRigidbodyFlags.Kinematic)
            {
                var massProperties = MassProperties.UnitSphere;
                baker.AddComponent(entity, PhysicsMass.CreateKinematic(massProperties));
            }
            
            baker.AddComponent(entity, new PhysicsVelocity());
            baker.AddSharedComponent(entity, new PhysicsWorldIndex());
            
            if(entityFlags == PhysicsBakingUtility.PhysicsEntityFlags.NewEntity)
            {
                baker.AddComponent(entity, LocalTransform.FromPositionRotation(component.transform.position, component.transform.rotation));
                baker.AddComponent(entity, new LocalToWorld { Value = float4x4.identity });
            }
            return entity;
        }
    }
}