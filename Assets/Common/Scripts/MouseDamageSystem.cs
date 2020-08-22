using thelebaron.damage;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Common.Scripts
{
    [UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class MouseDamageSystem : SystemBase
    {
        private EntityQuery healthQuery;
        private BuildPhysicsWorld buildPhysicsWorldSystem;
        const float k_MaxDistance = 100.0f;

        protected override void OnCreate()
        {
            buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            healthQuery = GetEntityQuery(typeof(Health));
        }

        struct MouseDamageJob : IJobChunk
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            public float3 RayOrigin;
            public float3 RayEnd;
            [ReadOnly]public EntityTypeHandle EntityTypeHandle;
            public ComponentTypeHandle<Health> HealthTypeHandle;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var entities = chunk.GetNativeArray(EntityTypeHandle);
                var healths = chunk.GetNativeArray(HealthTypeHandle);
                

                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var health = healths[i];
                    
                    var hit = CollisionWorld.CastRay(CameraRay(), out var raycastHit);

                    if (!hit)
                        continue;
                    if(raycastHit.RigidBodyIndex == -1)
                        continue;
                    
                    var hitentity = CollisionWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                    if (!hitentity.Equals(entity)) 
                        return;
                    
                    health.Value -= 10;

                    healths[i] = health;

                }
            }

            public RaycastInput CameraRay()
            {
                return new RaycastInput
                {
                    Start  = RayOrigin,
                    End    = RayEnd,
                    Filter = CollisionFilter.Default,
                };
            }
        }


        protected override void OnUpdate()
        {
            if(!Input.GetKey(KeyCode.Mouse0))
                return;
            
            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorldSystem.GetOutputDependency());
            
            Dependency = new MouseDamageJob
            {
                CollisionWorld   = buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                RayOrigin        = Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                RayEnd           = Camera.main.ScreenPointToRay(Input.mousePosition).origin + Camera.main.ScreenPointToRay(Input.mousePosition).direction * k_MaxDistance,
                EntityTypeHandle = GetEntityTypeHandle(),
                HealthTypeHandle = GetComponentTypeHandle<Health>()
            }.Schedule(healthQuery, Dependency);

            // race condition without this, one of the physics systems does not wait for system to complete
            // see https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/21e35e20075827d09f3b9f4b8da4613a345df18e/UnityPhysicsSamples/Assets/Common/Scripts/MousePick/MousePickBehaviour.cs
            Dependency.Complete();
        }
    }
}