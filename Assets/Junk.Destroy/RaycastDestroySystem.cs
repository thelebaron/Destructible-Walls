﻿using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Junk.Destroy
{
    public partial struct RaycastDestroySystem : ISystem
    {
        // A mouse pick collector which stores every hit. Based off the ClosestHitCollector
        [BurstCompile]
        public struct MousePickCollector : ICollector<RaycastHit>
        {
            public bool                   IgnoreTriggers;
            public bool                   IgnoreStatic;
            public NativeArray<RigidBody> Bodies;
            public int                    NumDynamicBodies;

            public bool  EarlyOutOnFirstHit => false;
            public float MaxFraction        { get; private set; }
            public int   NumHits            { get; private set; }

            private RaycastHit m_ClosestHit;
            public  RaycastHit Hit => m_ClosestHit;

            public MousePickCollector(float maxFraction, NativeArray<RigidBody> rigidBodies, int numDynamicBodies)
            {
                m_ClosestHit     = default(RaycastHit);
                MaxFraction      = maxFraction;
                NumHits          = 0;
                IgnoreTriggers   = true;
                IgnoreStatic     = true;
                Bodies           = rigidBodies;
                NumDynamicBodies = numDynamicBodies;
            }

            #region ICollector

            public bool AddHit(RaycastHit hit)
            {
                Assert.IsTrue(hit.Fraction <= MaxFraction);

                var isAcceptable = true;
                if (IgnoreStatic)
                {
                    isAcceptable = isAcceptable && (hit.RigidBodyIndex >= 0) && (hit.RigidBodyIndex < NumDynamicBodies);
                }
                if (IgnoreTriggers)
                {
                    isAcceptable = isAcceptable && hit.Material.CollisionResponse != CollisionResponsePolicy.RaiseTriggerEvents;
                }

                if (!isAcceptable)
                {
                    return false;
                }

                MaxFraction  = hit.Fraction;
                m_ClosestHit = hit;
                NumHits      = 1;
                return true;
            }

            #endregion
        }
        public const float k_MaxDistance = 100.0f;
        
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if(Mouse.current == null)
                return;

            var click = Mouse.current.leftButton.isPressed;
            if(!click)
                return;

            var collisionWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.CollisionWorld;
            // Create a ray from the mouse click position
            var          ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            var rayInput = new RaycastInput
            {
                Start  = ray.origin,
                End    = ray.origin + ray.direction * k_MaxDistance,
                Filter = CollisionFilter.Default,
            };

            
            if (!collisionWorld.CastRay(rayInput, out var hit))
                return;
            
            //Debug.Log($"Hit {hit.Entity} at {hit.Position}");
            if (state.EntityManager.HasComponent<FractureBaker.Fractured>(hit.Entity))
            {
                // enable
                state.EntityManager.SetComponentEnabled<FractureBaker.Fractured>(hit.Entity, true);
            }
        }
    }
}