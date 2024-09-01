using Junk.Entities;
using Junk.Health;
using Junk.Physics;
using Junk.Physics.Hybrid;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using static Junk.Physics.Hybrid.PhysicsBakingUtility;

namespace Junk.Gameplay.Hybrid
{
    /// <summary>
    /// Gameplay is a vague name..
    /// All data should be filled out by the scopa importer. Unsure if this should reside in this package or not.
    /// Note use prefilled trigger collisionfilter data for baking and simplification.
    /// </summary>
    [RequireComponent(typeof(LinkedEntityGroupAuthoring))]
    public class GameplayAuthoring : MonoBehaviour
    {
        public float GizmosAlpha = 1;
        
        #region AngleData
        public bool  HasAngle;
        public float Angle;
        #endregion
        
        #region Target
        public string TargetName;
        public bool   IsTarget;

        public void InitializeTarget()
        {
            IsTarget = true;
        }
        #endregion
        
        #region Triggers
        // Target data
        public string      Target;
        public GameObject[] Targets;
        
        // Button data
        [HideInInspector]
        public bool          IsButton;

        // Trigger data
        public Vector3     TriggerExtents;
        public Vector3     TriggerCenter;
        public bool        IsTrigger;
        
        // Collider data
        public Vector3 ColliderExtents;
        public Vector3 ColliderCenter;
        public bool    IsCollider;
        //public PhysicsCategoryTags colliderBelongsTo;
        //public PhysicsCategoryTags colliderCollidesWith;
        
        public bool          IsDestructible;
        
        // Health data
        public           bool                HasHealth;
        public           float               HealthValue;

        #endregion
        
        public class GameplayAuthoringBaker : Baker<GameplayAuthoring>
        {
            /// <summary>
            /// Reads a trenchbroom angle and converts it to a float3
            /// </summary>
            /// <param name="angle"></param>
            /// <returns></returns>
            public Vector3 ParseEntityAngle(float angle)
            {
                // Calculate the direction from the Euler angles
                Quaternion rotation  = Quaternion.Euler(0,angle,0);
                Vector3    direction = rotation * Vector3.forward;
                
                
                //bool       isDown;
                //bool       isUp;
                if (angle == -1)
                {
                    //isUp = true;
                    direction = new Vector3(0, 1, 0);
                }
                if (angle == -2)
                {
                    //isDown = true;
                    direction = new Vector3(0, -1, 0);
                }
                return direction;
            }
            
            public override void Bake(GameplayAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                if (authoring.IsTarget)
                {
                    AddComponent<Active>(entity, false);
                }
                
                if(authoring.IsButton)
                {
                    var triggerEntity = this.CreateTriggerEntity(entity, authoring, new AuthoringPhysicsData
                    {
                        Extents  = authoring.TriggerExtents,
                        Center   = authoring.TriggerCenter,
                        Mass     = 0,
                        Geometry = AuthoringPhysicsData.GeoType.Box,
                        Filter   = new CollisionFilter
                        {
                            BelongsTo    = Layers.EnvironmentTriggers(),
                            CollidesWith = Layers.OnlyCharacters()
                        }
                    }, PhysicsEntityFlags.SelfEntity);
                    
                    AddComponent<TriggerUse>(triggerEntity);
                    AddComponent<PhysicalButton>(triggerEntity, new PhysicalButton
                    {
                        Use             = false,
                        Direction       = -ParseEntityAngle(authoring.Angle),
                        InitialPosition = authoring.transform.localPosition,
                        PressDepth      = 0.03f,
                        PressSpeed      = 3
                    });
                    
                    // Adds the targets to a buffer
                    var targets = AddBuffer<Target>(triggerEntity);
                    foreach (var target in authoring.Targets)
                    {
                        targets.Add(new Target
                        {
                            Entity = GetEntity(target, TransformUsageFlags.Dynamic)
                        });
                    }
                }
                
                if(authoring.HasHealth)
                {
                    AddComponent<Destroy>(entity);
                    SetComponentEnabled<Destroy>(entity, false);
                    //AddComponent<SimpleDestroy>(entity);
                    
                    AddBuffer<HealthDamageBuffer>(entity);
                    AddComponent<HealthData>(entity, new HealthData
                    {
                        Value = new float3(authoring.HealthValue, authoring.HealthValue, 0)
                    });
                    var rigidbodyData = new AuthoringPhysicsData
                    {
                        Center   = authoring.ColliderCenter,
                        Extents  = authoring.ColliderExtents,
                        Mass     = 0,
                        Geometry = AuthoringPhysicsData.GeoType.Box,
                        Filter = new CollisionFilter
                        {
                            BelongsTo    = BakingLayers.EnvironmentStatic_KinematicBelongsTo().Value,
                            CollidesWith = BakingLayers.EnvironmentStatic_KinematicCollidesWith().Value,
                        }
                    };
                    var e = this.CreateRigidbodyEntity(entity, authoring, rigidbodyData, PhysicsBakingUtility.PhysicsEntityFlags.SelfEntity, PhysicsBakingUtility.PhysicsRigidbodyFlags.Kinematic);
                }
            }
            
            public void AddComponent<T>(Entity entity, bool enabled) where T : unmanaged, IComponentData, IEnableableComponent
            {
                AddComponent<T>(entity);
                SetComponentEnabled<T>(entity, enabled);
            }

            
            public void AddComponent<T>(Entity entity, T data, bool requirement) where T : unmanaged, IComponentData
            {
                if (requirement) 
                    AddComponent(entity, data);
            }
            
            public void AddComponent(Entity entity, ComponentType componentType, bool requirement)
            {
                if (requirement) 
                    AddComponent(entity, componentType);
            }
        }

        private void OnDrawGizmos()
        {
            var color   = Color.blue;
            color.a      = GizmosAlpha;
            Gizmos.color = color;
            if(IsTrigger)
                Gizmos.DrawCube(transform.position + TriggerCenter, TriggerExtents);
            
            color   = Color.green;
            if(IsCollider)
                Gizmos.DrawCube(transform.position + ColliderCenter, ColliderExtents);
        }
    }
}