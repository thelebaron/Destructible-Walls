
using Junk.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Junk.Physics
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct DoorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        private struct TriggerJob: ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<Player> Player;
            [ReadOnly] public ComponentLookup<PlayerCommands> PlayerCommands;
            public ComponentLookup<TriggerUse> DoorTrigger;
        
            public void Execute(TriggerEvent triggerEvent)
            {
                var otherEntity = triggerEvent.EntityA;
                var triggerEntity = triggerEvent.EntityB;

                if (!DoorTrigger.HasComponent(triggerEntity))
                {
                    //Debug.Log($"DoorTriggerJob !DoorTrigger.HasComponent");
                    return;
                }
            
                var trigger = DoorTrigger[triggerEntity];
                var playerEntity = Player.HasComponent(otherEntity);

                FoundPlayer(triggerEvent.EntityA, triggerEvent.EntityB);
                FoundPlayer(triggerEvent.EntityB, triggerEvent.EntityA);
                
                if (playerEntity && PlayerCommands.HasComponent(otherEntity))
                {
                    //Debug.Log($"triggerjob player logic ");
                    if (PlayerCommands[otherEntity].Interact)
                    {
                        trigger.Use = true;
                        //Debug.Log($"DoorTriggerJob use");
                        DoorTrigger[triggerEntity] = trigger;
                    }
                }
            }
            private void FoundPlayer(Entity entity, Entity otherEntity)
            {
                if (Player.HasComponent(entity))
                {
                    //Debug.Log($"DoorTriggerJob Found player on entity {entity}");
                    //Debug.Log($"DoorTriggerJob !DoorTrigger.HasComponent { DoorTrigger.HasComponent(otherEntity)}");
                }
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var fixedDeltaTime = SystemAPI.Time.fixedDeltaTime;
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            state.Dependency = new TriggerJob
            {
                Player         = SystemAPI.GetComponentLookup<Player>(true),
                PlayerCommands = SystemAPI.GetComponentLookup<PlayerCommands>(true),
                DoorTrigger    = SystemAPI.GetComponentLookup<TriggerUse>(),
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

        }
    }
}