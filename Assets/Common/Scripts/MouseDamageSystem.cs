using Junk.Entities;
using Junk.Hitpoints;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace Common.Scripts
{
    public partial class MouseDamageSystem : SystemBase
    {
        private EntityQuery healthQuery;
        const float k_MaxDistance = 100.0f;

        protected override void OnCreate()
        {
            healthQuery = GetEntityQuery(typeof(HealthData));
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
            if (!Input.GetKey(KeyCode.Mouse0)) 
                return;

            var collisionWorld = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.CollisionWorld;
                
            collisionWorld.CastRay(CameraRay(), out var raycastHit);
            if(raycastHit.RigidBodyIndex == -1)
                return;
                    
            var entity = collisionWorld.Bodies[raycastHit.RigidBodyIndex].Entity;
                
            if (entity.HasComponent<HealthData>(this))
            {
                var health = entity.GetComponent<HealthData>(this);
                health.Value -= 10;
                entity.SetComponent(health, this);
            }
        }
    }
}