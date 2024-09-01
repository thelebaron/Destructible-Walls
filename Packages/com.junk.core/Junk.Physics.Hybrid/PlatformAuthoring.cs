using System;
using Junk.Math;
using Junk.Physics.Stateful;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.GraphicsIntegration;
using Unity.Transforms;
using UnityEngine;
using PhysMaterial = Unity.Physics.Material;
using Unity.CharacterController;

namespace Junk.Physics.Hybrid
{
    public class PlatformAuthoring : MonoBehaviour
    {
        public bool  HideGizmos;
        public float Delay  = 1.25f;
        public float ReturnDelay = 2.5f;
        public float Height;
        public float Speed;
        
        [Header("Trigger")]
        public float3              TriggerOffset       = float3.zero;
        public Bounds              TriggerBounds       = new Bounds(new Vector3(2, 2, 2), new Vector3(2, 1, 2));
        public PhysicsCategoryTags TriggerBelongsTo    = PhysicsEnvironmentTags.TriggerBelongsTo;
        public PhysicsCategoryTags TriggerCollidesWith = PhysicsEnvironmentTags.TriggerCollidesWith;
        
        [Header("Collider")]
        public float3 ColliderPosition = float3.zero;
        public Bounds ColliderBounds = new Bounds(new Vector3(2, 0, 2), new Vector3(2, 0.5f, 2));
        public PhysicsCategoryTags ColliderBelongsTo = PhysicsEnvironmentTags.TriggerBelongsTo; // use same as trigger
        public PhysicsCategoryTags ColliderCollidesWith = PhysicsEnvironmentTags.ColliderCollidesWith;
        
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            var endPosition = ColliderPosition + (float3)transform.position + (float3)ColliderBounds.center;
            endPosition.y += Height;
            Gizmos.DrawWireCube(endPosition, ColliderBounds.size);
            
            // Calculate the corners of the current position
            Vector3 currentCenter = (float3)transform.position + (float3)ColliderBounds.center;
            Vector3 halfSize      = TriggerBounds.size * 0.5f;

            Vector3[] currentCorners = new Vector3[8];
            currentCorners[0] = currentCenter + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            currentCorners[1] = currentCenter + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            currentCorners[2] = currentCenter + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            currentCorners[3] = currentCenter + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            currentCorners[4] = currentCenter + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            currentCorners[5] = currentCenter + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            currentCorners[6] = currentCenter + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            currentCorners[7] = currentCenter + new Vector3(halfSize.x, halfSize.y, halfSize.z);

            // Calculate the corners of the end position
            Vector3[] endCorners = new Vector3[8];
            endCorners[0] = endPosition + new float3(-halfSize.x, -halfSize.y, -halfSize.z);
            endCorners[1] = endPosition + new float3(-halfSize.x, -halfSize.y, halfSize.z);
            endCorners[2] = endPosition + new float3(-halfSize.x, halfSize.y, -halfSize.z);
            endCorners[3] = endPosition + new float3(-halfSize.x, halfSize.y, halfSize.z);
            endCorners[4] = endPosition + new float3(halfSize.x, -halfSize.y, -halfSize.z);
            endCorners[5] = endPosition + new float3(halfSize.x, -halfSize.y, halfSize.z);
            endCorners[6] = endPosition + new float3(halfSize.x, halfSize.y, -halfSize.z);
            endCorners[7] = endPosition + new float3(halfSize.x, halfSize.y, halfSize.z);

            // Draw lines between corresponding corners
            Gizmos.color = Color.cyan;
            for (int i = 0; i < 8; i++)
            {
                Gizmos.DrawLine(currentCorners[i], endCorners[i]);
            }
        }

        public void OnDrawGizmosSelected()
        {
            if (HideGizmos)
                return;
            
            // Draw collider
            Gizmos.color = Color.green;
            Gizmos.DrawCube(ColliderPosition + (float3)transform.position + (float3)ColliderBounds.center, ColliderBounds.size);
            
            // Draw trigger
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(TriggerOffset + (float3)transform.position + (float3)TriggerBounds.center, TriggerBounds.size);
            Gizmos.color = new Color(0, 1, 1, 0.75f);
            Gizmos.DrawCube(TriggerOffset + (float3)transform.position + (float3)TriggerBounds.center, TriggerBounds.size);

        }
    }

    public class PlatformBaker : Baker<PlatformAuthoring>
    {
        public override void Bake(PlatformAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            
            var endPosition = authoring.ColliderPosition + (float3)authoring.transform.position + (float3)authoring.ColliderBounds.center;
            endPosition.y += authoring.Height;

            var platform = Platform.Default;
            platform.Delay            = authoring.Delay;
            platform.Speed            = authoring.Speed;
            platform.OriginalPosition = authoring.transform.position;
            platform.TargetPosition   = endPosition;
            
            AddComponent(entity, platform);
            AddComponent<StatefulTriggerEvent>(entity);

            var currentTransform = new RigidTransform(authoring.transform.rotation, authoring.transform.position);
            var trackedTransform = new TrackedTransform
            {
                CurrentFixedRateTransform = currentTransform,
                PreviousFixedRateTransform = currentTransform,
            };

            AddComponent(GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace), trackedTransform);

            // Create start trigger
            // this is the trigger that the player will collide with to start the platform
            CreateTrigger(entity, authoring, authoring.TriggerBelongsTo, authoring.TriggerCollidesWith, authoring.TriggerBounds, authoring.TriggerOffset, " player", out var startTriggerEntity);
            AddComponent(startTriggerEntity, new PlatformTrigger
            {
                PhysicsMoverEntity = entity,
                TriggerType = PhysicsMoverTriggerType.Start,
                Delay = authoring.Delay,
            });
            // Create collider
            CreateKinematicRigidbody(entity, authoring, authoring.ColliderBelongsTo, authoring.ColliderCollidesWith, authoring.ColliderBounds, authoring.ColliderPosition);
        }

        /// <summary>
        /// Trigger creation for trigger queries
        /// </summary>
        private void CreateTrigger(Entity mainEntity, PlatformAuthoring authoring, PhysicsCategoryTags belongsTo,
            PhysicsCategoryTags collidesWith, Bounds bounds, float3 translationOffset, string extraname, out Entity entityTrigger)
        {
            entityTrigger = CreateAdditionalEntity(TransformUsageFlags.WorldSpace | TransformUsageFlags.Dynamic, false, authoring.gameObject.name + extraname + " + Trigger");

            var filter = CollisionFilter.Default;
            filter.BelongsTo = belongsTo.Value;
            filter.CollidesWith = collidesWith.Value;

            var material = PhysMaterial.Default;
            material.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;

            var blob = Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                BevelRadius = 0.05f,
                Center = bounds.center,
                Orientation = quaternion.identity,
                Size = bounds.size,
            }, filter, material);
            var physicsCollider = new PhysicsCollider { Value = blob };

            AddBlobAsset(ref physicsCollider.Value, out var blobhash);
            // Link this trigger to the weapon 
            AddComponent(entityTrigger, physicsCollider);
            AddSharedComponent(entityTrigger, new PhysicsWorldIndex());
            AddComponent<StatefulTriggerEvent>(entityTrigger);
            
            // Add transform data
            AddComponent(entityTrigger,
                new MoverTriggerBakingData { LocalTransform = LocalTransform.FromPositionRotation((float3)authoring.transform.position + translationOffset, quaternion.identity) });
        }

        /// <summary>
        /// Weapon mesh collider & rigidbody creation
        /// </summary>
        private void CreateKinematicRigidbody(Entity entity, PlatformAuthoring authoring, PhysicsCategoryTags belongsTo,
            PhysicsCategoryTags collidesWith, Bounds bounds, float3 translationOffset)
        {
            var filter = CollisionFilter.Default;
            filter.BelongsTo = belongsTo.Value;
            filter.CollidesWith = collidesWith.Value;

            var material = PhysMaterial.Default;
            material.CollisionResponse = CollisionResponsePolicy.Collide;
            var massdist = new MassDistribution
            {
                Transform = new RigidTransform(quaternion.identity, float3.zero),
                InertiaTensor = new float3(2f / 5f)
            };
            var massProperties = new MassProperties
            {
                AngularExpansionFactor = 0,
                MassDistribution = massdist,
                Volume = 1
            };

            var blob = Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                BevelRadius = 0.05f,
                Center = bounds.center,
                Orientation = quaternion.identity,
                Size = bounds.size,
            }, filter, material);
            var massKinematic = PhysicsMass.CreateKinematic(blob.Value.MassProperties);
            var physicsCollider = new PhysicsCollider { Value = blob };

            AddBlobAsset(ref physicsCollider.Value, out var blobhash);
            //physicsCollider.Value.Value.MassProperties = massProperties;

            AddComponent(entity, physicsCollider);
            AddComponent(entity, massKinematic);
            AddComponent(entity, new PhysicsVelocity());
            AddSharedComponent(entity, new PhysicsWorldIndex());
            // smoothing for physics
            AddComponent<PhysicsGraphicalSmoothing>(entity);
            AddComponent<PhysicsGraphicalInterpolationBuffer>(entity);
// endpointcollider position
// var translation = (float3)authoring.transform.position + authoring.TriggerPosition + authoring.MoveAxis * authoring.Distance;

            var translation = (float3)authoring.transform.position + authoring.ColliderPosition;
            var localTransform = LocalTransform.FromPositionRotation(translation, quaternion.identity);
            var localToWorld = float4x4.TRS(localTransform.Position, quaternion.identity, 1);


            AddComponent(entity, new MoverKinematicBakingData
            {
                LocalTransform = localTransform,
                LocalToWorld = new LocalToWorld
                {
                    Value = localToWorld
                }
            });
        }
    }

    public static class PhysicsEnvironmentTags
    {
        public static readonly PhysicsCategoryTags TriggerBelongsTo = new PhysicsCategoryTags
        {
            Category01 = true,
        };

        public static readonly PhysicsCategoryTags TriggerCollidesWith = new PhysicsCategoryTags
        {
            Category03 = true,
            Category06 = true,
        };

        public static readonly PhysicsCategoryTags ColliderCollidesWith = new PhysicsCategoryTags
        {
            Category00 = true,
            Category01 = true,
            Category02 = true,
            Category03 = true,
            Category04 = true,
            Category05 = true,
            Category06 = true,
            Category08 = true,
        };
    }

    [TemporaryBakingType]
    public struct MoverTriggerBakingData : IComponentData
    {
        public CollisionFilter CollisionFilter;
        public LocalTransform LocalTransform;
    }

    [TemporaryBakingType]
    public struct MoverKinematicBakingData : IComponentData
    {
        public LocalTransform LocalTransform;
        public LocalToWorld LocalToWorld;
    }


    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct PhysicsMoverBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            /*foreach (var (physicsCollider, moverBakingData) in SystemAPI.Query<PhysicsCollider, MoverBakingData>())
            {
                physicsCollider.Value.Value.SetCollisionFilter(moverBakingData.CollisionFilter);
                physicsCollider.Value.Value.SetCollisionResponse(CollisionResponsePolicy.RaiseTriggerEvents);
            }*/

            // Set transform data for the platform/mover trigger
            foreach (var (moverBakingData, localTransform) in SystemAPI.Query<RefRO<MoverTriggerBakingData>, RefRW<LocalTransform>>())
            {
                localTransform.ValueRW = moverBakingData.ValueRO.LocalTransform;
            }

            // Set transform data for the kinematic platform/mover
            foreach (var (moverBakingData, localTransform, localToWorld) in SystemAPI.Query<RefRO<MoverKinematicBakingData>, RefRW<LocalTransform>, RefRW<LocalToWorld>>())
            {
                localTransform.ValueRW = moverBakingData.ValueRO.LocalTransform;
                localToWorld.ValueRW = moverBakingData.ValueRO.LocalToWorld;
            }
        }
    }
}