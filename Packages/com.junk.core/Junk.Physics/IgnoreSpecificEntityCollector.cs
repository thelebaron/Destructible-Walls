using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace Junk.Physics
{
    // A mouse pick collector which stores every hit. Based off the ClosestHitCollector
    [BurstCompile]
    public struct IgnoreSpecificEntityCollector : ICollector<RaycastHit>
    {
        private readonly Entity ignoreEntity;

        public bool       EarlyOutOnFirstHit => false;
        public float      MaxFraction        { get; private set; }
        public int        NumHits            { get; private set; }
        public RaycastHit Hit                { get; private set; }

        public IgnoreSpecificEntityCollector(Entity ignoreEntity, float maxFraction)
        {
            this.ignoreEntity = ignoreEntity;
            Hit = default(RaycastHit);
            MaxFraction = maxFraction;
            NumHits = 0;
        }

        public bool AddHit(RaycastHit hit)
        {
            if (hit.Entity.Equals(ignoreEntity))
                return false;
            
            MaxFraction = hit.Fraction;
            Hit = hit;
            NumHits = 1;
            
            // if ignore static / kinematic(are kinematic considered dynamic?> maybe)
            /*
            Assert.IsTrue(hit.Fraction < MaxFraction);

            var isAcceptable = (hit.RigidBodyIndex >= 0) && (hit.RigidBodyIndex < NumDynamicBodies);
            if(IgnoreTriggers)
            {
                var body = Bodies[hit.RigidBodyIndex];
                isAcceptable = isAcceptable && !ColliderUtils.IsTrigger(body.Collider, hit.ColliderKey);
            }

            if (!isAcceptable)
            {
                return false;
            }*/
            return true;
        }
    }
    
    /// <summary>
    /// Only returns a hit if the raycast hits a specific entity
    /// </summary>
    [BurstCompile]
    public struct RaycastSpecificEntityCollector : ICollector<RaycastHit>
    {
        public Entity Target;

        public bool  EarlyOutOnFirstHit => false;
        public float MaxFraction        { get; private set; }
        public int   NumHits            { get; private set; }

        private RaycastHit m_ClosestHit;
        public  RaycastHit Hit => m_ClosestHit;

        public RaycastSpecificEntityCollector(Entity entity)
        {
            Target = entity;
            m_ClosestHit = default;
            MaxFraction  = 1;
            NumHits      = 1;
        }

        public bool AddHit(RaycastHit hit)
        {
            m_ClosestHit = hit;
            return hit.Entity.Equals(Target);
        }
    }
    
    
    [BurstCompile]
    public struct ExplosionColliderCollector : ICollector<ColliderCastHit>
    {
        //public ComponentDataFromEntity<Health> HealthFromEntity;
        public  ComponentLookup<PhysicsVelocity> PhysicsVelocityFromEntity;
        public  ComponentLookup<PhysicsMass>     PhysicsMassFromEntity;
        public  NativeList<ColliderCastHit>              ColliderCastHits;
        private PhysicsWorld                             m_World;
        public  bool                                     EarlyOutOnFirstHit => false;
        public  float                                    MaxFraction        { get; private set; }
        public  int                                      NumHits            { get; private set; }

        private ColliderCastHit m_ClosestHit;
        public ColliderCastHit Hit => m_ClosestHit;

        public ExplosionColliderCollector(PhysicsWorld world, float maxFraction, ComponentLookup<PhysicsVelocity> velocityEntity,
            ComponentLookup<PhysicsMass> massEntity, ref NativeList<ColliderCastHit> colliderCastHits)
        {
            m_World                   = world;
            m_ClosestHit              = default(ColliderCastHit);
            MaxFraction               = maxFraction;
            NumHits                   = 0;
            PhysicsVelocityFromEntity = velocityEntity;
            PhysicsMassFromEntity     = massEntity;
            ColliderCastHits          = colliderCastHits;
        }

        #region ICollector

        public bool AddHit(ColliderCastHit hit)
        {
            if(!PhysicsVelocityFromEntity.HasComponent(hit.Entity))
                return false;
            
            ColliderCastHits.Add(hit);
            NumHits++;
            return true;
        }

        #endregion

    }
    
    
    [BurstCompile]
    public struct NoTriggerRigidbodyCollector : ICollector<RaycastHit>
    {
        public  Entity         IgnoreEntity;
        private CollisionWorld m_World;
        
        public bool                   IgnoreTriggers;
        public NativeSlice<RigidBody> Bodies;
        public int                    NumDynamicBodies;

        public bool  EarlyOutOnFirstHit => false;
        public float MaxFraction        { get; private set; }
        public int   NumHits            { get; private set; }

        private RaycastHit m_ClosestHit;
        public  RaycastHit Hit => m_ClosestHit;

        public NoTriggerRigidbodyCollector(Entity ignoreEntity, CollisionWorld world, float maxFraction, NativeSlice<RigidBody> rigidBodies, int numDynamicBodies)
        {
            IgnoreEntity     = ignoreEntity;
            m_World          = world;
            m_ClosestHit     = default(RaycastHit);
            MaxFraction      = maxFraction;
            NumHits          = 0;
            IgnoreTriggers   = true;
            Bodies           = rigidBodies;
            NumDynamicBodies = numDynamicBodies;
        }

        #region ICollector

        public bool AddHit(RaycastHit hit)
        {
            if (hit.Entity.Equals(IgnoreEntity))
                return false;
            
            MaxFraction  = hit.Fraction;
            m_ClosestHit = hit;
            NumHits      = 1;
            
            // if ignore static / kinematic(are kinematic considered dynamic?> maybe)
            /*
            Assert.IsTrue(hit.Fraction < MaxFraction);

            var isAcceptable = (hit.RigidBodyIndex >= 0) && (hit.RigidBodyIndex < NumDynamicBodies);
            if(IgnoreTriggers)
            {
                var body = Bodies[hit.RigidBodyIndex];
                isAcceptable = isAcceptable && !ColliderUtils.IsTrigger(body.Collider, hit.ColliderKey);
            }

            if (!isAcceptable)
            {
                return false;
            }*/
            return true;
        }

        #endregion
    }

}