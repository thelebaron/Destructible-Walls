using System;
using Junk.Entities;
using Junk.Physics.Stateful;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;

namespace Junk.Physics
{
    [Serializable]
    public struct Platform : IComponentData, IEnableableComponent
    {
        public PhysicsMoverState State;
        public float  Delay;
        
        public float3 TranslationAxis;
        public float TranslationAmplitude;
        public float TranslationSpeed;
        public float RotationSpeed;
        public float3 RotationAxis;

        [HideInInspector]
        public bool IsInitialized;
        [HideInInspector]
        public float3 OriginalPosition;
        [HideInInspector]
        public quaternion OriginalRotation;
        
        // make default
        public static Platform Default => new Platform
        {
            State = PhysicsMoverState.Stopped,
            Delay = 0f,
            TranslationAxis = math.down(),
            TranslationAmplitude = 10f,
            TranslationSpeed = 1f,
            RotationSpeed = 0f,
            RotationAxis = float3.zero,
            IsInitialized = false,
            OriginalPosition = float3.zero,
            OriginalRotation = quaternion.identity,
        };
    }
    
    /// <summary>
    /// When a player collides with this trigger, it will start the PhysicsMover.
    /// </summary>
    public struct PlatformTrigger : IComponentData, IEnableableComponent
    {
        public Entity                  PhysicsMoverEntity;
        public PhysicsMoverTriggerType TriggerType;
        public float                   Delay;
    }
    
    public enum PhysicsMoverState
    {
        Stopped,
        Moving, // Moving to end position
        Returning, // Returning to start position
    }
    public enum PhysicsMoverTriggerType
    {
        Start,
        Return,
        Stop,
    }

    [UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    public partial struct PlatformSystem : ISystem
    {
        private EntityQuery triggerQuery;
        private EntityQuery platformQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            triggerQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<StatefulTriggerEvent>()
                    .WithAll<PlatformTrigger>()
                    .Build(ref state);
            
            platformQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Platform>()
                .WithAllRW<PhysicsVelocity>()
                .WithAll<PhysicsMass>()
                .WithAll<LocalTransform>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            float invDeltaTime = 1f / deltaTime;
            float time = (float)SystemAPI.Time.ElapsedTime;
            
            state.Dependency = new PlatformTriggerEventJob
            {
                PlayerLookup               = SystemAPI.GetComponentLookup<Player>(),
                PlatformLookup         = SystemAPI.GetComponentLookup<Platform>(),
                PlatformTriggerLookup  = SystemAPI.GetComponentLookup<PlatformTrigger>(true),
                PhysicsVelocityLookup      = SystemAPI.GetComponentLookup<PhysicsVelocity>(),
                LocalTransformLookup       = SystemAPI.GetComponentLookup<LocalTransform>(),
                StatefulTriggerEventLookup = SystemAPI.GetBufferLookup<StatefulTriggerEvent>(true),
                EntityType           = SystemAPI.GetEntityTypeHandle(),
                StatefulTriggerEventType = SystemAPI.GetBufferTypeHandle<StatefulTriggerEvent>(true),
                PhysicsMoverTriggerDataType = SystemAPI.GetComponentTypeHandle<PlatformTrigger>(true),
            }.Schedule(triggerQuery, state.Dependency);
            
            state.Dependency = new PlatformMoveJob
            {
                DeltaTime                 = deltaTime,
                InvDeltaTime              = invDeltaTime,
                ElapsedTime               = time,
                PhysicsMoverTypeHandle    = SystemAPI.GetComponentTypeHandle<Platform>(),
                PhysicsVelocityTypeHandle = SystemAPI.GetComponentTypeHandle<PhysicsVelocity>(),
                PhysicsMassTypeHandle     = SystemAPI.GetComponentTypeHandle<PhysicsMass>(true),
                LocalTransformTypeHandle  = SystemAPI.GetComponentTypeHandle<LocalTransform>(true),
            }.Schedule(platformQuery, state.Dependency);
        }

        [BurstCompile]
        public struct PlatformMoveJob : IJobChunk
        {
            public            float                                DeltaTime;
            public            float                                InvDeltaTime;
            public            float                                ElapsedTime;
            public            ComponentTypeHandle<Platform>    PhysicsMoverTypeHandle;
            public            ComponentTypeHandle<PhysicsVelocity> PhysicsVelocityTypeHandle;
            [ReadOnly] public ComponentTypeHandle<PhysicsMass>     PhysicsMassTypeHandle;
            [ReadOnly] public ComponentTypeHandle<LocalTransform>  LocalTransformTypeHandle;
            
            static float3 CalculateTargetPosition(LocalTransform transform, float3 targetPos, float maxSpeed, float deltaTime)
            {
                // Calculate the direction towards the target
                float3 direction = math.normalize(targetPos - transform.Position);

                // Calculate the distance to the target
                float distance = math.distance(transform.Position, targetPos);

                // Calculate the distance that can be traveled in this frame with the max speed
                float maxDistance = maxSpeed * deltaTime;

                // If the distance to the target is less than what can be traveled in this frame, 
                // then just set the current position to the target position
                if (distance <= maxDistance)
                {
                    return targetPos;
                }
                else
                {
                    // Calculate the new position by moving towards the target with the max speed
                    return transform.Position + direction * maxDistance;
                }
            }
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var physicsMovers     = chunk.GetNativeArray(ref PhysicsMoverTypeHandle);
                var physicsVelocities = chunk.GetNativeArray(ref PhysicsVelocityTypeHandle);
                var physicsMasses     = chunk.GetNativeArray(ref PhysicsMassTypeHandle);
                var localTransforms   = chunk.GetNativeArray(ref LocalTransformTypeHandle);
                
                var entityEnumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (entityEnumerator.NextEntityIndex(out var i))
                {
                    var enabled   = chunk.IsComponentEnabled(ref PhysicsMoverTypeHandle, i);
                    var platform  = physicsMovers[i];
                    var mass      = physicsMasses[i];
                    var transform = localTransforms[i];
                    var velocity  = physicsVelocities[i];
                    
                    if(!platform.IsInitialized)
                    {
                        // Remember initial pos/rot, because our calculations depend on them
                        //platform.OriginalPosition = transform.Position;
                        //platform.OriginalRotation = transform.Rotation;
                        platform.IsInitialized = true;
                    }

                    if(platform.Delay > 0)
                    {
                        platform.Delay       -= DeltaTime;
                        velocity.Linear      =  float3.zero;
                        physicsVelocities[i] =  velocity;
                        physicsMovers[i] = platform;
                        return;
                    }
                    
                    if (platform.State == PhysicsMoverState.Moving)
                    {
                        var    targetPos = CalculateTargetPosition(transform, platform.OriginalPosition + 
                            (math.normalizesafe(platform.TranslationAxis) * platform.TranslationAmplitude), 
                            platform.TranslationSpeed, DeltaTime);

                        var rotationFromMovement = quaternion.Euler(math.normalizesafe(platform.RotationAxis) * platform.RotationSpeed * ElapsedTime);
                        var targetRot = math.mul(rotationFromMovement, platform.OriginalRotation);

                        // Move with velocity
                        velocity = PhysicsVelocity.CalculateVelocityToTarget(in mass, transform.Position, transform.Rotation, new RigidTransform(targetRot, targetPos), InvDeltaTime);

                    }
                    if (platform.State == PhysicsMoverState.Returning)
                    {
                        var    targetPos         =CalculateTargetPosition(transform, platform.OriginalPosition, platform.TranslationSpeed, DeltaTime);

                        quaternion rotationFromMovement = quaternion.Euler(math.normalizesafe(platform.RotationAxis) * platform.RotationSpeed * ElapsedTime);
                        quaternion targetRot = math.mul(rotationFromMovement, platform.OriginalRotation);

                        // Move with velocity
                        velocity = PhysicsVelocity.CalculateVelocityToTarget(in mass, transform.Position, transform.Rotation, new RigidTransform(targetRot, targetPos), InvDeltaTime);
                    }
                    if (platform.State == PhysicsMoverState.Stopped)
                    {
                        // lerp to zero
                        velocity.Linear = math.lerp(velocity.Linear, float3.zero, DeltaTime * 15f);
                    }
                    
                    physicsVelocities[i] = velocity;
                }
            }
        }
        
        [BurstCompile]
        public struct PlatformTriggerEventJob : IJobChunk
        {
            public            ComponentLookup<Player>            PlayerLookup;
            [ReadOnly] public ComponentLookup<PlatformTrigger>   PlatformTriggerLookup;
            public            ComponentLookup<Platform>          PlatformLookup;
            public            ComponentLookup<PhysicsVelocity>   PhysicsVelocityLookup;
            public            ComponentLookup<LocalTransform>    LocalTransformLookup;
            [ReadOnly] public BufferLookup<StatefulTriggerEvent> StatefulTriggerEventLookup;
            
            [ReadOnly] public EntityTypeHandle                       EntityType;
            [ReadOnly] public BufferTypeHandle<StatefulTriggerEvent> StatefulTriggerEventType;
            [ReadOnly] public ComponentTypeHandle<PlatformTrigger> PhysicsMoverTriggerDataType;
            
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var physicsMoverTriggerDatas = chunk.GetNativeArray(ref PhysicsMoverTriggerDataType);
                var statefulTriggerEvents = chunk.GetBufferAccessor(ref StatefulTriggerEventType);
                
                var entityEnumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (entityEnumerator.NextEntityIndex(out var i))
                {
                    var entity             = entities[i];
                    var triggerData = physicsMoverTriggerDatas[i];
                    var triggerEvents      = statefulTriggerEvents[i];
                    //var enabled             = chunk.IsComponentEnabled(ref StatefulTriggerEventType, i);
                    
                    for (var j = 0; j < triggerEvents.Length; j++)
                    {
                        ProcessTriggerEvents(j, entity, triggerData, triggerEvents);
                    }
                }
            }
            
            private void ProcessTriggerEvents(int   index, Entity entity, PlatformTrigger trigger,
                DynamicBuffer<StatefulTriggerEvent> triggerEvents)
            {
                var triggerEvent = triggerEvents[index];
                var otherEntity  = triggerEvent.GetOtherEntity(entity);
                //Debug.Log("Enter" + otherEntity);
                var moverEntity = PlatformTriggerLookup[entity].PhysicsMoverEntity;
                var platform       = PlatformLookup[moverEntity];
                
                switch (trigger.TriggerType)
                {
                    // if the trigger is a start trigger, and the mover is idle, set the mover to moving
                    case PhysicsMoverTriggerType.Start:
                    {
                        if (triggerEvent.State == StatefulEventState.Enter) // && !emitterEnabled)
                        {
                            if (platform.State == PhysicsMoverState.Stopped)
                            {
                                platform.State                     = PhysicsMoverState.Moving;
                                platform.Delay                     = trigger.Delay;
                                PlatformLookup[moverEntity] = platform;
                            }
                        }
                    }
                    break;
                    
                    // if the trigger is a return trigger, and the mover is moving, set the mover to waiting to return
                    case PhysicsMoverTriggerType.Return:

                        if (triggerEvent.State == StatefulEventState.Enter) // && !emitterEnabled)
                        {
                            if (platform.State == PhysicsMoverState.Moving)
                            {
                                platform.State                     = PhysicsMoverState.Returning;
                                platform.Delay                     = trigger.Delay;
                                PlatformLookup[moverEntity] = platform;
                            }
                        }
                        break;
                    
                    // if the trigger is a stop trigger, and the mover is moving, set the mover to waiting to return
                    case PhysicsMoverTriggerType.Stop:
                        if (triggerEvent.State == StatefulEventState.Enter) // && !emitterEnabled)
                        {
                            if (platform.State == PhysicsMoverState.Returning)
                            {
                                platform.State                     = PhysicsMoverState.Stopped;
                                platform.Delay                     = trigger.Delay;
                                PlatformLookup[moverEntity] = platform;
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                

            }
        }
    }
}