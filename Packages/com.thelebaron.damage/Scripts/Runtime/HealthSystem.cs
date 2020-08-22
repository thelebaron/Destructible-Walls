using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace thelebaron.damage
{
    /// <summary>
    /// See TRSToLocalToWorldSystem / TRSToLocalToParentSystem
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class HealthSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
        private EntityQuery                            damageEventsQuery;
        private EntityQuery healthQuery;
        //private EntityQuery                            historyQuery;

        protected override void OnCreate()
        {
            endSimulationEntityCommandBufferSystem            = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            damageEventsQuery = GetEntityQuery(ComponentType.ReadOnly<DamageInstance>());
            
            healthQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite<HealthFrameBuffer>(),
                },
                Any = new ComponentType[]
                {
                    ComponentType.ReadWrite<Health>(),
                    ComponentType.ReadWrite<HealthState>(),
                    ComponentType.ReadWrite<HealthLink>(),
                    ComponentType.ReadWrite<CompositeHealth>(),
                },
                None = new ComponentType[]
                {
                    ComponentType.ReadOnly<Dead>()
                },
                Options = EntityQueryOptions.FilterWriteGroup
            });
            //healthQuery = GetEntityQuery(ComponentType.ReadWrite<Health>(), ComponentType.ReadWrite<HealthBuffer>(), ComponentType.Exclude<Dead>());
        }


        /// <summary>
        /// Adds the damage events to a buffer, and then destroys them. They get processed in the following job.
        /// </summary>
        [BurstCompile]
        private struct AddToBufferJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;
            [ReadOnly] public EntityTypeHandle EntityType;
            [ReadOnly] public ComponentTypeHandle<DamageInstance> DamageEventType;
            [NativeDisableParallelForRestriction] public BufferFromEntity<HealthFrameBuffer> HealthFrameBufferBfe;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntityType);
                var damageEvents = chunk.GetNativeArray(DamageEventType);
                
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var damageEvent = damageEvents[i];
                    
                    if (HealthFrameBufferBfe.HasComponent(damageEvent.Receiver))
                        HealthFrameBufferBfe[damageEvent.Receiver].Add(damageEvent);
                    
                    // Destroy damage event entity
                    CommandBuffer.DestroyEntity(chunkIndex, entity);
                }
            }
        }

        
        /// <summary>
        /// Applies the damage to the health component. Todo: merge damage and apply in one go? so can be gibbed?
        /// </summary>
        [BurstCompile]
        private struct ApplyHealthJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;
            public float ElapsedTime;
            public ComponentTypeHandle<Health> HealthTypeHandle;
            [ReadOnly] public ComponentTypeHandle<HealthLink> HealthLinkTypeHandle;
            public BufferTypeHandle<HealthFrameBuffer> HealthBufferTypeHandle;
            public ComponentTypeHandle<HealthState> HealthStateTypeHandle;
            public uint LastSystemVersion;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var changed = chunk.DidOrderChange(LastSystemVersion) || chunk.DidChange(HealthBufferTypeHandle, LastSystemVersion);
                if (!changed)
                {
                    return;
                }
                
                var chunkHealths = chunk.GetNativeArray(HealthTypeHandle);
                var chunkHealthLinks = chunk.GetNativeArray(HealthLinkTypeHandle);
                var chunkHealthFrameBuffers = chunk.GetBufferAccessor(HealthBufferTypeHandle);
                var chunkHealthStates = chunk.GetNativeArray(HealthStateTypeHandle);
                var hasHealth = chunk.Has(HealthTypeHandle);
                var hasLinkedHealth = chunk.Has(HealthLinkTypeHandle);
                var hasState = chunk.Has(HealthStateTypeHandle);
                var count = chunk.Count;
                
                if (hasHealth)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var health = chunkHealths[i];
                        var frameBuffer = chunkHealthFrameBuffers[i];
                        
                        // Skip if dead or no events in buffer
                        if(health.Value <= 0 || frameBuffer.Length < 1)
                            continue;
                        
                        var senderEntity = Entity.Null;
                        var totalDamage = 0;
                        var lastDamage = 0;
                        
                        // Get sum total of all the damage in the buffer
                        for (var j = 0; j < frameBuffer.Length; j++)
                        {
                            senderEntity =  frameBuffer[j].Value.Sender;
                            lastDamage   =  frameBuffer[j].Value.Value;
                            totalDamage  += frameBuffer[j].Value.Value;
                        }
                        
                        // Clear the damage buffer
                        frameBuffer.Clear();
                        
                        // Update info(deprecated for below)
                        health.LastDamageValue   = lastDamage;
                        health.LastDamagerEntity = senderEntity;
                        
                        
                        if (hasState)
                        {
                            // Update per frame changes from damage events
                            var state = chunkHealthStates[i];
                            state.LastDamagerEntity = senderEntity;
                            state.LastDamageValue   = lastDamage;
                            state.TimeLastHurt      = ElapsedTime;
                            // Zero out damage, a bit hacky but should work without needing complexity
                            if (state.Invulnerable)
                                totalDamage = 0;
                            
                            chunkHealthStates[i] = state;
                        }
                        
                        // Subtract total damage from health
                        health.Value -= totalDamage;
                        chunkHealths[i] = health;
                    }
                }
                
                if (hasLinkedHealth)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var link = chunkHealthLinks[i];
                        var frameBuffer = chunkHealthFrameBuffers[i];
                        
                        // If no damage events in the buffer, skip
                        if(frameBuffer.Length < 1)
                            continue;
                        
                        var senderEntity = Entity.Null;
                        var totalDamage = 0;
                        var lastDamage = 0;
                        
                        // Get sum total of all the damage in the buffer
                        for (var j = 0; j < frameBuffer.Length; j++)
                        {
                            var damageInstance = frameBuffer[j].Value;
                            damageInstance.Value = (int)link.Multiplier * damageInstance.Value;
                            damageInstance.Receiver = link.Value;

                            var damageEntity = CommandBuffer.CreateEntity(chunkIndex);
                            CommandBuffer.AddComponent(chunkIndex, damageEntity, damageInstance);

                            senderEntity =  frameBuffer[j].Value.Sender;
                            lastDamage   =  frameBuffer[j].Value.Value;
                            totalDamage  += frameBuffer[j].Value.Value;
                        }
                        
                        // Clear the damage buffer
                        frameBuffer.Clear();
                        
                        if (hasState)
                        {
                            // Update per frame changes from damage events
                            var state = chunkHealthStates[i];
                            state.LastDamagerEntity = senderEntity;
                            state.LastDamageValue   = lastDamage;
                            state.TimeLastHurt      = ElapsedTime;
                            // Zero out damage, a bit hacky but should work without needing complexity
                            if (state.Invulnerable)
                                totalDamage = 0;
                            
                            chunkHealthStates[i] = state;
                        }
                        
                        // Subtract total damage from health
                        //health.Value -= totalDamage;
                        //chunkHealths[i] = health;
                    }
                }
                
            }
        }
 
        
        protected override void OnUpdate()
        {
            Dependency = new AddToBufferJob
            {
                CommandBuffer      = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityType         = GetEntityTypeHandle(),
                DamageEventType    = GetComponentTypeHandle<DamageInstance>(true),
                HealthFrameBufferBfe = GetBufferFromEntity<HealthFrameBuffer>()
            }.ScheduleSingle(damageEventsQuery, Dependency);
            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

            Dependency = new ApplyHealthJob
            {
                CommandBuffer           = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                ElapsedTime             = (float)Time.ElapsedTime,
                HealthTypeHandle        = GetComponentTypeHandle<Health>(),
                HealthLinkTypeHandle    = GetComponentTypeHandle<HealthLink>(true),
                HealthBufferTypeHandle  = GetBufferTypeHandle<HealthFrameBuffer>(),
                HealthStateTypeHandle   = GetComponentTypeHandle<HealthState>(),
                LastSystemVersion = LastSystemVersion
            }.ScheduleSingle(healthQuery, Dependency);
            endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}