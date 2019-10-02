using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
/* 
namespace Common.Scripts
{
    public class TestSystem : JobComponentSystem
    {
        private BuildPhysicsWorld  m_BuildPhysicsWorldSystem;
        private StepPhysicsWorld   m_StepPhysicsWorld;
        private ExportPhysicsWorld m_ExportPhysicsWorld;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_BuildPhysicsWorldSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            m_StepPhysicsWorld        = World.Active.GetOrCreateSystem<StepPhysicsWorld>();
            m_ExportPhysicsWorld      = World.Active.GetOrCreateSystem<ExportPhysicsWorld>();
        }
        
        //[BurstCompile]
        struct RaycastJob : IJob
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            
            public void Execute()
            {
                var c0 = new Translation();
                
                var rayInput = new RaycastInput
                {
                    Start = c0.Value, End = c0.Value + new float3(0,-10,0), Filter = CollisionFilter.Default
                };

                var hit = CollisionWorld.CastRay(rayInput, out var rayHit);
                // Check the collisionworld for a hit
                if (hit)
                {
                    if (rayHit.RigidBodyIndex <= 0)
                        return;

                    Debug.Log("hit");
                }
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var finalPhysicsHandle      = JobHandle.CombineDependencies(inputDeps, m_BuildPhysicsWorldSystem.FinalJobHandle);

            var job = new RaycastJob
            {
                CollisionWorld = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld
            };

            var rayHandle = job.Schedule(finalPhysicsHandle);
            rayHandle.Complete();
            
            return rayHandle;

        }

        
    }
}*/