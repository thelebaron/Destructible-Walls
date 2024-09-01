using Junk.Entities;
using Junk.Gameplay.Hybrid;
using Junk.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using static Junk.Gameplay.Hybrid.GameplayDataUtility;

namespace Junk.Physics.Hybrid
{
    public class DoorBaker : Baker<DoorAuthoring>
    {
        public override void Bake(DoorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity colliderEntity;

            var rendererExtents = authoring.GetBoundsExtents();
            
            //var mapComponent = authoring.GetComponent<MapComp>()
            // check if a collider authoring setup exists
            if (authoring.GetComponent<PhysicsShapeAuthoring>() == null)
            {
                //colliderEntity = BakeColliderEntity(authoring);
                var rigidbodyData = new AuthoringPhysicsData
                {
                    Center   = authoring.colliderOffset,
                    Extents  = authoring.colliderShape,
                    Mass     = 0,
                    Geometry = AuthoringPhysicsData.GeoType.Box,
                    Filter = new CollisionFilter
                    {
                        BelongsTo    = authoring.colliderBelongsTo.Value,
                        CollidesWith = authoring.colliderCollidesWith.Value,
                    }
                };
                colliderEntity = this.CreateRigidbodyEntity(entity, authoring, rigidbodyData, PhysicsBakingUtility.PhysicsEntityFlags.NewEntity, PhysicsBakingUtility.PhysicsRigidbodyFlags.Kinematic);
            }
            else
                colliderEntity = entity;
            
            /////////////////////////////////////
            // Trigger for trigger queries
            /////////////////////////////////////

            var selfTrigger = true;
            // if we are a target of another trigger(button, another door, etc), dont create a trigger
            if (authoring.GetComponent<GameplayAuthoring>())
            {
                if(authoring.GetComponent<GameplayAuthoring>().IsTarget)
                    selfTrigger = false;
            }

            // If we dont have an external trigger, create one
            if (selfTrigger)
            {
                //var trigger = BakeTriggerEntity(authoring);
                var trigger = this.CreateTriggerEntity(entity, authoring, new AuthoringPhysicsData
                {
                    Center   = authoring.triggerCenter,
                    Extents  = authoring.triggerDimensions,
                    Mass     = 0,
                    Geometry = AuthoringPhysicsData.GeoType.Box,
                    Filter   = new CollisionFilter
                    {
                        BelongsTo = authoring.triggerBelongsTo.Value,
                        CollidesWith = authoring.triggerCollidesWith.Value,
                    }
                }, PhysicsBakingUtility.PhysicsEntityFlags.NewEntity);

                // Link both entities together
                AddComponent<TriggerUse>(trigger);
                var targets = AddBuffer<Target>(trigger);
                targets.Add(new Target { Entity = entity });
                AddComponent<Active>(entity);
                SetComponentEnabled<Active>(entity, false);
            }



            /////////////////////////////////////
            // Door
            /////////////////////////////////////

            // Precalculate the closed position, using the extents and the direction
            var openPosition = authoring.transform.position;
            var direction      = authoring.direction;
            var extents        = authoring.extents * 2;
            var lip = new float3(direction.x *  + authoring.lip, direction.y *  + authoring.lip, direction.z *  + authoring.lip);
            extents.x    *= direction.x + lip.x;
            extents.y    *= direction.y + lip.x;
            extents.z    *= direction.z + lip.x;
            openPosition += extents;
            
            AddComponent(entity, new Door
            {
                MovementType                  = authoring.movementType,
                ColliderEntity                = colliderEntity,
                OpenPosition                  = openPosition,
                ClosedPosition                = authoring.transform.position,
                Direction                     = authoring.direction,
                Speed                         = authoring.speed,
                MovementDuration              = authoring.duration,
                Axis                          = authoring.rotationAxis,
                NegativeDirection             = authoring.negativeDirection,
                RotationInOppositeOfTriggerer = authoring.rotationInOppositeOfTriggerer
            });
            AddComponent(entity, new RotationEulerXYZ());
            AddComponent(entity, new CompositeRotation());
        }

        private Entity BakeColliderEntity(DoorAuthoring authoring)
        {
            var colliderEntity = CreateAdditionalEntity(TransformUsageFlags.ManualOverride, false, authoring.name + " Trigger");
            AddComponent(colliderEntity, LocalTransform.FromPositionRotation(authoring.transform.position, authoring.transform.rotation));
            AddComponent(colliderEntity, new LocalToWorld { Value = float4x4.identity });
            var colliderFilter = CollisionFilter.Default;
            colliderFilter.BelongsTo    = authoring.colliderBelongsTo.Value;
            colliderFilter.CollidesWith = authoring.colliderCollidesWith.Value;
            var colliderMaterial = Unity.Physics.Material.Default;
            var colliderBlob = Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                BevelRadius = 0.001f,
                Center      = authoring.colliderOffset,
                Orientation = quaternion.identity,
                Size        = authoring.colliderShape
            }, colliderFilter, colliderMaterial);
            var colliderMass = new Unity.Physics.MassProperties();

            AddSharedComponent(colliderEntity, new PhysicsWorldIndex());
            AddComponent(colliderEntity, new PhysicsCollider { Value = colliderBlob });
            AddComponent(colliderEntity, PhysicsMass.CreateKinematic(colliderMass));
            return colliderEntity;
        }
    }
}