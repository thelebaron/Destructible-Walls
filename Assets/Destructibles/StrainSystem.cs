using thelebaron.Damage;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{
    
    /// <summary>
    /// An static anchor prevents a physicsvelocity from being added to an entity. 
    /// </summary>
    public struct StaticAnchor : IComponentData
    {

    }
    /// <summary>
    /// An anchor prevents a physicsvelocity from being added to an entity. 
    /// </summary>
    public struct DynamicAnchor : IComponentData
    {

    }
    
    public class StrainSystem : JobComponentSystem
    {
        
        /// <summary>
        /// If a node has no immediate anchor, detach it. For now set health value so we can burst this job.
        /// </summary>
        [BurstCompile]
        [RequireComponentTag(typeof(Node), typeof(DynamicAnchor))]
        [ExcludeComponent(typeof(PhysicsVelocity),typeof(StaticAnchor))]
        struct CheckDynamicAnchors : IJobForEachWithEntity_EBC<Connection, Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly] public ComponentDataFromEntity<DynamicAnchor> DynamicAnchorData;
            
            public void Execute(Entity entity, int index, DynamicBuffer<Connection> connection, ref Health health)
            {
                bool removeAnchor = true;
                
                for (int i = 0; i < connection.Length; i++)
                {
                    if (DynamicAnchorData.Exists(connection[i].Node))
                    {
                        removeAnchor = false;
                    }
                }

                if (removeAnchor)
                    health.Value = 0; //EntityCommandBuffer.RemoveComponent(index, entity, typeof(DynamicAnchor));
            }
        }
        
        [RequireComponentTag(typeof(Node), typeof(DynamicAnchor))]
        [ExcludeComponent(typeof(Connection),typeof(PhysicsVelocity),typeof(StaticAnchor))]
        struct BreakConnection : IJobForEachWithEntity<Translation>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Translation c0)
            {
                EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
            }
        }
        
        [RequireComponentTag(typeof(Node), typeof(DynamicAnchor), typeof(Connection))]
        [ExcludeComponent(typeof(PhysicsVelocity),typeof(StaticAnchor))]
        private struct DetachNodeJob : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public void Execute(Entity entity, int index, ref Health health)
            {
                if (health.Value <= 0)
                {
                    EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
                    EntityCommandBuffer.RemoveComponent(index, entity, typeof(Connection));
                    EntityCommandBuffer.RemoveComponent(index, entity, typeof(DynamicAnchor));
                }
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var breakConnectionJob = new BreakConnection
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            var breakConnectionHandle = breakConnectionJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(breakConnectionHandle);
            
            var detachNodeJob = new DetachNodeJob{ EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent() };
            var detachNodeHandle = detachNodeJob.Schedule(this, breakConnectionHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(detachNodeHandle);

            return detachNodeHandle;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEntityCommandBufferSystem =
                World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        private EndSimulationEntityCommandBufferSystem m_EndSimulationEntityCommandBufferSystem;
    }
}