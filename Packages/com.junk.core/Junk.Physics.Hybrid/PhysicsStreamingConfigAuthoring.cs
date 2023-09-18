using Junk.Entities;
using Unity.Entities;
using UnityEngine;

namespace Junk.Physics.Hybrid
{
    // add to camera entity
    public class PhysicsStreamingConfigAuthoring : MonoBehaviour
    {
        public float DistanceForStreamingIn  = 60;
        public float DistanceForStreamingOut = 80;
    }
    
    public class PhysicsStreamingConfigBaker : Baker<PhysicsStreamingConfigAuthoring>
    {
        public override void Bake(PhysicsStreamingConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PhysicsStreamingConfig
            {
                DistanceForStreamingIn  = authoring.DistanceForStreamingIn,
                DistanceForStreamingOut = authoring.DistanceForStreamingOut
            });
        }
    }
}