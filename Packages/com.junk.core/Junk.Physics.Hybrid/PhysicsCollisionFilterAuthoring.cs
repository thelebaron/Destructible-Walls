using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Junk.Physics.Hybrid
{
    public class PhysicsCollisionFilterAuthoring : MonoBehaviour
    {
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;

        public class PhysicsCollisionFilterAuthoringBaker : Baker<PhysicsCollisionFilterAuthoring>
        {
            public override void Bake(PhysicsCollisionFilterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new PhysicsCollisionFilterBakingData
                    {
                        CollisionFilter = new CollisionFilter
                        {
                            BelongsTo    = authoring.BelongsTo.Value,
                            CollidesWith = authoring.CollidesWith.Value,
                            GroupIndex   = 0,
                        }
                    });
            }
        }
    }

    [BakingType]
    public struct PhysicsCollisionFilterBakingData : IComponentData
    {
        public CollisionFilter CollisionFilter;
    }
    
    
}