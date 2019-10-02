using thelebaron.Damage;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Destructibles
{
    public class AnchorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new AnchoredNode());
        }
    }
    
    /// <summary>
    /// An anchor prevents a physicsvelocity from being added to an entity. 
    /// </summary>
    public struct AnchoredNode : IComponentData
    {

    }

    
    public class FractureWorkSystem : JobComponentSystem
    {
        [RequireComponentTag(typeof(FractureNode))]
        [ExcludeComponent(typeof(Connection),typeof(PhysicsVelocity),typeof(AnchoredNode))]
        struct BreakConnection : IJobForEachWithEntity<Translation>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            public void Execute(Entity entity, int index, ref Translation c0)
            {
                EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
            }
        }
        
        
            
        [ExcludeComponent(typeof(PhysicsVelocity),typeof(AnchoredNode))]
        private struct DetachNodeJob : IJobForEachWithEntity_EBC<Connection, Health>
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            public void Execute(Entity entity, int index, DynamicBuffer<Connection> graph, ref Health health)
            {
                if (health.Value <= 0)
                {
                    EntityCommandBuffer.AddComponent(index, entity, new PhysicsVelocity());
                    EntityCommandBuffer.RemoveComponent(index, entity, typeof(Connection));
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