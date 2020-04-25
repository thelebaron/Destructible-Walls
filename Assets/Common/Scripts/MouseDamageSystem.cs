using thelebaron.damage;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Common.Scripts
{
    public class MouseDamageSystem : JobComponentSystem
    {
        private EntityQuery m_MouseGroup;
        private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
        const float k_MaxDistance = 100.0f;

        protected override void OnCreate()
        {
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            m_MouseGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] {typeof(MouseDamage)}
            });
        }

        private struct ClickJob : IJobForEach<MouseDamage>
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            public RaycastInput RayInput;
            public float3 Forward;
            public bool Damage;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Health> HealthDataFromEntity;

            public void Execute(ref MouseDamage c0)
            {
                if(!Damage)
                    return;
                
                var hit = CollisionWorld.CastRay(RayInput, out var raycastHit);

                if (hit)
                {
                    if (raycastHit.RigidBodyIndex != -1)
                    {
                        
                        var hitentity = CollisionWorld.Bodies[raycastHit.RigidBodyIndex].Entity;

                        if (hitentity.Equals(Entity.Null)) return;

                        
                        if (HealthDataFromEntity.Exists(hitentity))
                        {
                            Debug.Log("Damaged " + hitentity + "!");
                            var health = HealthDataFromEntity[hitentity];
                            health.Value -= 10;
                            HealthDataFromEntity[hitentity] = health;
                        }
                    }
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (Camera.main == null)
                return inputDeps;
            
            var finalhandle = JobHandle.CombineDependencies(inputDeps, m_BuildPhysicsWorldSystem.FinalJobHandle);
            
            var clickhandle = new ClickJob
            {
                CollisionWorld = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                RayInput = new RaycastInput
                {
                    Start = Camera.main.ScreenPointToRay(Input.mousePosition).origin,
                    End = Camera.main.ScreenPointToRay(Input.mousePosition).origin + Camera.main.ScreenPointToRay(Input.mousePosition).direction * k_MaxDistance,
                    Filter = CollisionFilter.Default,
                },
                Forward = Camera.main.transform.forward,
                Damage = Input.GetMouseButtonDown(0),
                HealthDataFromEntity = GetComponentDataFromEntity<Health>(false)
                
            }.Schedule(this, finalhandle);

            clickhandle.Complete();
            
            return clickhandle;
        }
    }
}