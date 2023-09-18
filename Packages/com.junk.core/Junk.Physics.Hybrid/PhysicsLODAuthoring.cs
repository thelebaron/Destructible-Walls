using Junk.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Entities
{
    public class PhysicsLODAuthoring : MonoBehaviour
    {
        public float DistanceForStreamingIn  = 60;
        public float DistanceForStreamingOut = 80;
    }
    
    public class PhysicsLODBaker : Baker<PhysicsLODAuthoring>
    {
        public override void Bake(PhysicsLODAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PhysicsLOD
            {
                DistanceForStreamingIn  = authoring.DistanceForStreamingIn,
                DistanceForStreamingOut = authoring.DistanceForStreamingOut
            });
            
            var position = authoring.transform.position;
            var hash     = HashedPosition(position, 100); //(int)math.hash(new int3(math.floor(localToWorld.Position / cellRadius)));
            AddComponent(entity, new PhysicsPositionHash
            {
                HashValue = hash
            });
        }
        
        private static int HashedPosition(float3 position, float cellRadius)
        {
            var hashPos = (int)math.hash(new int3(math.floor(position / cellRadius)));
            return hashPos;
        }
    }
}