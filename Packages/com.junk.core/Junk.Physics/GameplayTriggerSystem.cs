using Junk.Entities;
using Junk.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Physics
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct GameplayTriggerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }

        /// <summary>
        /// Reads trigger events and sets the trigger command to true if the player is interacting with the trigger.
        /// </summary>
        [BurstCompile]
        private struct ReadTriggerEventsJob: ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<Player>         PlayerLookup;
            [ReadOnly] public ComponentLookup<PlayerCommands> PlayerCommandsLookup;
            public            ComponentLookup<TriggerUse>     TriggerUseLookup;
        
            public void Execute(TriggerEvent triggerEvent)
            {
                var otherEntity = triggerEvent.EntityA;
                var triggerEntity = triggerEvent.EntityB;
                
                //TriggerUse(triggerEvent);
                //FoundPlayer(triggerEvent.EntityA, triggerEvent.EntityB);
                //FoundPlayer(triggerEvent.EntityB, triggerEvent.EntityA);
                
                if (triggerEntity.HasComponent(TriggerUseLookup))
                {
                    //Debug.Log($"DoorTriggerJob !DoorTrigger.HasComponent");
                    var triggerUse      = TriggerUseLookup[triggerEntity];
                    var playerEntity = PlayerLookup.HasComponent(otherEntity);

                    FoundPlayer(triggerEvent.EntityA, triggerEvent.EntityB);
                    FoundPlayer(triggerEvent.EntityB, triggerEvent.EntityA);

                    if (playerEntity && PlayerCommandsLookup.HasComponent(otherEntity))
                    {
                        //Debug.Log($"triggerjob player logic ");
                        if (PlayerCommandsLookup[otherEntity].Interact)
                        {
                            //Debug.Log($"TriggerCommand.Use ");
                            triggerUse.Use = true;
                            TriggerUseLookup[triggerEntity] = triggerUse;
                        }
                    }
                }
            }
            
            private void TriggerUse(TriggerEvent triggerEvent)
            {
                //Debug.Log($"TriggerUse a{ triggerEvent.EntityA } a{ triggerEvent.EntityB }");
                if (TriggerUseLookup.HasComponent(triggerEvent.EntityA))
                {
                    //Debug.Log($"TriggerUseLookup entity a");
                    //Debug.Log($"DoorTriggerJob !DoorTrigger.HasComponent { DoorTrigger.HasComponent(otherEntity)}");
                }
                
                if (TriggerUseLookup.HasComponent(triggerEvent.EntityB))
                {
                    //Debug.Log($"TriggerUseLookup  entity b");
                    //Debug.Log($"DoorTriggerJob !DoorTrigger.HasComponent { DoorTrigger.HasComponent(otherEntity)}");
                }
            }
            
            private void FoundPlayer(Entity entity, Entity otherEntity)
            {
                if (PlayerLookup.HasComponent(entity))
                {
                    //Debug.Log($"DoorTriggerJob Found player on entity {entity} with other {otherEntity}");
                    
                }
            }

            private void ProcessTrigger(Entity entity)
            {
                //Debug.Log($"DoorTriggerJob ProcessTrigger");
            }
        }
        
        [BurstCompile]
        private partial struct AnimatePhysicalButtonJob : IJobEntity
        {
            public float                   DeltaTime;

            public void Execute(Entity entity, in TriggerUse triggerUse, ref PhysicalButton physicalButton, ref LocalTransform transform)
            {
                if (triggerUse.Use)
                {
                    physicalButton.Use = true;
                }

                if (physicalButton.Use)
                {
                    Vector3 targetPosition = physicalButton.InitialPosition - physicalButton.Direction * physicalButton.PressDepth;
                    transform.Position = math.lerp(transform.Position, targetPosition, DeltaTime       * physicalButton.PressSpeed);
                    
                    if(maths.approximately(transform.Position, targetPosition))
                    {
                        physicalButton.Use = false;
                    }
                }
                if (!physicalButton.Use)
                {
                    transform.Position = math.lerp(transform.Position, physicalButton.InitialPosition, DeltaTime * physicalButton.PressSpeed);
                }
            }
        }
        
        [BurstCompile]
        private partial struct ActivateTriggerTargetJob : IJobEntity
        {
            public float                   DeltaTime;
            public float                   FixedDeltaTime;
            public ComponentLookup<Active> ActiveLookup;

            public void Execute(Entity entity, in TriggerUse triggerUse, DynamicBuffer<Target> triggerTargets)
            {
                if (triggerUse.Use)
                {
                    var targets = triggerTargets.AsNativeArray();
                    foreach (var target in targets)
                    {
                        //Debug.Log($"ActivateTriggerTargetJob target {target.Entity}");
                        ActiveLookup.SetComponentEnabled(target.Entity, true);
                    }
                }
            }
        }
        
        [BurstCompile]
        private partial struct ClearTriggerUseJob : IJobEntity
        {
            public void Execute(ref TriggerUse triggerUse)
            {
                if (triggerUse.Use)
                {
                    // Clear use command
                    triggerUse.Use = false;
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime      = SystemAPI.Time.DeltaTime;
            var fixedDeltaTime = SystemAPI.Time.fixedDeltaTime;
            
            state.Dependency = new ReadTriggerEventsJob
            {
                PlayerLookup         = SystemAPI.GetComponentLookup<Player>(true),
                PlayerCommandsLookup = SystemAPI.GetComponentLookup<PlayerCommands>(true),
                TriggerUseLookup     = SystemAPI.GetComponentLookup<TriggerUse>(),
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            state.Dependency = new ActivateTriggerTargetJob
            {
                DeltaTime            = deltaTime,
                FixedDeltaTime       = fixedDeltaTime,
                ActiveLookup    = SystemAPI.GetComponentLookup<Active>()
            }.Schedule(state.Dependency);
            
            state.Dependency = new AnimatePhysicalButtonJob
            {
                DeltaTime      = deltaTime
            }.Schedule(state.Dependency);
            
            state.Dependency = new DoorJob
            {
                DeltaTime            = deltaTime,
                DoorTriggerLookup    = SystemAPI.GetComponentLookup<TriggerUse>(),
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                LocalToWorldLookup   = SystemAPI.GetComponentLookup<LocalToWorld>()
            }.Schedule(state.Dependency);
            
            state.Dependency = new ClearTriggerUseJob().Schedule(state.Dependency);
        }
    }
}