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
    

    
    public class StrainSystem : JobComponentSystem
    {
        /*
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
        }*/
        
        [RequireComponentTag(typeof(Anchored))]
        [ExcludeComponent(typeof(PhysicsVelocity))] //, typeof(StaticAnchor)
        private struct CheckHealth : IJobForEachWithEntity<Health, Node>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Health health, ref Node node)
            {
                if (health.Value <= 0)
                {
                    //EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
                    EntityCommandBuffer.AddComponent(index, entity, new Unanchored());
                    EntityCommandBuffer.RemoveComponent(index, entity, typeof(Anchored));

                    var e = EntityCommandBuffer.CreateEntity(index);
                    
                    EntityCommandBuffer.AddComponent(index, e, new BreakEvent
                    {
                        NodeEntity = entity,
                        GraphEntity = node.Graph
                    });
                    //EntityCommandBuffer.RemoveComponent(index, entity, typeof(Connection));
                    //EntityCommandBuffer.RemoveComponent(index, entity, typeof(DynamicAnchor));
                }
            }
        }
        
        [RequireComponentTag(typeof(Node), typeof(Unanchored))]
        [ExcludeComponent(typeof(PhysicsVelocity))]
        struct AddVelocity : IJobForEachWithEntity<Translation>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Translation c0)
            {
                EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var checkHealthJob = new CheckHealth{ EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent() };
            var checkHealthHandle = checkHealthJob.Schedule(this, inputDeps);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(checkHealthHandle);
            
            var addVelocityJob = new AddVelocity { EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()};
            var addVelocityHandle = addVelocityJob.Schedule(this, checkHealthHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(addVelocityHandle);
            
            
            /*
            var unanchorChildGraphJob = new UnanchorChildGraph
            {
                EntityCommandBuffer = m_EndSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                Anchored = GetComponentDataFromEntity<AnchoredNode>(true)
            };
            var unanchorChildGraphHandle = unanchorChildGraphJob.Schedule(this, checkHealthHandle);
            m_EndSimulationEntityCommandBufferSystem.AddJobHandleForProducer(unanchorChildGraphHandle);
            */

            return addVelocityHandle;
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

/*        
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
        }*/