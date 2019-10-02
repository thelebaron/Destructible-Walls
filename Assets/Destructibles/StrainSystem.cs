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
    
    
    public struct AnchoredNode : IComponentData
    {

    }
    public struct UnanchoredNode : IComponentData
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
        
        [RequireComponentTag(typeof(Node), typeof(UnanchoredNode))]
        [ExcludeComponent(typeof(PhysicsVelocity))]
        struct AddVelocity : IJobForEachWithEntity<Translation>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Translation c0)
            {
                EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
            }
        }
        


        private struct UnanchorChildGraph : IJobForEachWithEntity_EBC<GraphChild, PhysicsVelocity>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [ReadOnly]public ComponentDataFromEntity<AnchoredNode> Anchored;
            public void Execute(Entity entity, int index, DynamicBuffer<GraphChild> graphChildren, ref PhysicsVelocity c1)
            {
                for (int i = 0; i < graphChildren.Length; i++)
                {
                    if (Anchored.Exists(graphChildren[i].Node))
                    {
                        EntityCommandBuffer.RemoveComponent(index, graphChildren[i].Node, typeof(AnchoredNode));
                        EntityCommandBuffer.AddComponent(index, graphChildren[i].Node, new UnanchoredNode());
                    }
                }
            }
        }
        
        [RequireComponentTag(typeof(Node), typeof(AnchoredNode))]
        [ExcludeComponent(typeof(PhysicsVelocity))] //, typeof(StaticAnchor)
        private struct UnanchorDeadNode : IJobForEachWithEntity<Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Health health)
            {
                if (health.Value <= 0)
                {
                    //EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
                    EntityCommandBuffer.AddComponent(index, entity, new UnanchoredNode());
                    EntityCommandBuffer.RemoveComponent(index, entity, typeof(AnchoredNode));
                    //EntityCommandBuffer.RemoveComponent(index, entity, typeof(Connection));
                    //EntityCommandBuffer.RemoveComponent(index, entity, typeof(DynamicAnchor));
                }
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var addVelocityJob = new AddVelocity
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            var addVelocityHandle = addVelocityJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(addVelocityHandle);
            
            var unanchorDeadNodeJob = new UnanchorDeadNode{ EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent() };
            var unanchorDeadNodeHandle = unanchorDeadNodeJob.Schedule(this, addVelocityHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(unanchorDeadNodeHandle);

            var unanchorChildGraphJob = new UnanchorChildGraph
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Anchored = GetComponentDataFromEntity<AnchoredNode>(true)
            };
            var unanchorChildGraphHandle = unanchorChildGraphJob.Schedule(this, unanchorDeadNodeHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(unanchorChildGraphHandle);
            

            return unanchorChildGraphHandle;
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