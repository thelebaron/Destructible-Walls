using thelebaron.bee;
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

        public RaycastInput CameraRay()
        {
            return new RaycastInput
            {
                Start  = Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                End    = Camera.main.ScreenPointToRay(Input.mousePosition).origin + Camera.main.ScreenPointToRay(Input.mousePosition).direction * k_MaxDistance,
                Filter = CollisionFilter.Default,
            };
        }

        protected override void OnUpdate()
        {
            Dependency = buildPhysicsWorldSystem.GetOutputDependency();
            Dependency.Complete();

            if (!Input.GetKey(KeyCode.Mouse0)) 
                return;
            
            var collisionWorld = buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;
                
            collisionWorld.CastRay(CameraRay(), out var raycastHit);
            if(raycastHit.RigidBodyIndex == -1)
                return;
                    
            var entity = collisionWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                
            if (entity.HasComponent<Health>(this))
            {
                var health = entity.GetComponent<Health>(this);
                health.Value -= 10;
                entity.SetComponent(health, this);
            }
        }
    }
}