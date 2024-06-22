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
        public bool HideGizmos = false;
        public float StartDelay = 1.25f;
        public float ReturnDelay = 2.5f;

        [Header("Platform")]
        public float3              TranslationAxis;
        public float               TranslationAmplitude;
        public float               TranslationSpeed;
        public float               RotationSpeed;
        public float3              RotationAxis;
        
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
        
        public float3 CalculateEndTargetPosition(Vector3 originalPosition, Vector3 translationAxis, float translationSpeed, float translationAmplitude)
        {
            // Calculate the displacement based on sinusoidal motion
            var displacement = translationAmplitude * translationAxis.normalized;

            // Calculate the end target position
            var endTargetPosition = originalPosition + displacement;

            return endTargetPosition;
        }
        public void OnDrawGizmos()
        {
            float3 originalPosition = transform.position;
            var    axis             = TranslationAxis;
            var    speed            = TranslationSpeed;
            var    amplitude        = TranslationAmplitude;
            var    time             = Time.time;
            var    endTargetPos     = originalPosition + (math.normalizesafe(axis) * math.sin(time * speed) * TranslationAmplitude);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(CalculateEndTargetPosition(originalPosition, axis, speed, amplitude) + TriggerOffset, TriggerBounds.size);
        }

        public void OnDrawGizmosSelected()
        {
            if (HideGizmos)
                return;
            // Draw trigger
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(TriggerOffset + (float3)transform.position + (float3)TriggerBounds.center, TriggerBounds.size);
            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Gizmos.DrawCube(TriggerOffset + (float3)transform.position + (float3)TriggerBounds.center, TriggerBounds.size);

            // Draw collider
            Gizmos.color = Color.green;
            Gizmos.DrawCube(ColliderPosition + (float3)transform.position + (float3)ColliderBounds.center, ColliderBounds.size);

            // Draw endpoint
            //Assert.IsTrue(IsSingleAxis(Platform.TranslationAxis));

            // Draw endpoint
            {
                var displacement = math.normalizesafe(TranslationAxis) * TranslationAmplitude;
                var endPosition = (float3)transform.position + TriggerOffset + displacement;
                // Draw trigger
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(endPosition + (float3)TriggerBounds.center, TriggerBounds.size);

                Gizmos.color = new Color(1, 1, 0, 0.25f);
                Gizmos.DrawCube( endPosition + (float3)TriggerBounds.center, TriggerBounds.size);
            }
        }

        // Method returns true if only one axis is not zero of a float3
        private static bool IsSingleAxis(float3 value)
        {
            var count = 0;
            if (value.x != 0) count++;
            if (value.y != 0) count++;
            if (value.z != 0) count++;

            if (count != 1)
                throw new System.Exception("MoveAxis must have only one non-zero axis.");

            return true;
        }
    }

    public class PlatformBaker : Baker<PlatformAuthoring>
    {
        public override void Bake(PlatformAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            var platform = Platform.Default;
            platform.State                = PhysicsMoverState.Stopped;
            platform.Delay                = authoring.StartDelay;
            platform.TranslationAxis      = authoring.TranslationAxis;
            platform.TranslationAmplitude = authoring.TranslationAmplitude;
            platform.TranslationSpeed     = authoring.TranslationSpeed;
            platform.RotationSpeed        = authoring.RotationSpeed;
            platform.RotationAxis         = authoring.RotationAxis;
            platform.OriginalPosition     = authoring.transform.position;
            platform.OriginalRotation     = authoring.transform.rotation;
            
            AddComponent(entity, platform);
            AddComponent<StatefulTriggerEvent>(entity);

            RigidTransform currentTransform = new RigidTransform(authoring.transform.rotation, authoring.transform.position);
            TrackedTransform trackedTransform = new TrackedTransform
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
                Delay = authoring.StartDelay,
            });

            // Create return trigger
            // this is the trigger that the platform will collide with to return the platform
            var returnpointCollidesWith = PhysicsEnvironmentTags.TriggerBelongsTo;

            var displacement = math.normalizesafe(authoring.TranslationAxis) * authoring.TranslationAmplitude;
            CreateTrigger(entity, authoring, authoring.TriggerBelongsTo, returnpointCollidesWith, authoring.TriggerBounds,
                authoring.TriggerOffset + displacement, " return", out var returnTriggerEntity);
            AddComponent(returnTriggerEntity, new PlatformTrigger
            {
                PhysicsMoverEntity = entity,
                TriggerType = PhysicsMoverTriggerType.Return,
                Delay = authoring.ReturnDelay,
            });

            // Create stop trigger
            // this is the trigger that the platform will collide with to stop the platform
            // theres some oddity with stop trigger position when its positive on the y axis
            var stopCollidesWith = returnpointCollidesWith;
            var triggerOffset    = authoring.TriggerOffset;
            var triggerBounds    = authoring.TriggerBounds;
            if (authoring.TranslationAxis.y.IsPositive())
            {
                triggerOffset += new float3(0, -1.5f, 0);
                var center = triggerBounds.center;
                center.y             = -center.y;
                triggerBounds.center = center;
            }
            
            CreateTrigger(entity, authoring, authoring.TriggerBelongsTo, stopCollidesWith, triggerBounds, triggerOffset, " stop", out var stopTriggerEntity);
            AddComponent(stopTriggerEntity, new PlatformTrigger
            {
                PhysicsMoverEntity = entity,
                TriggerType = PhysicsMoverTriggerType.Stop,
                Delay = 0,
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