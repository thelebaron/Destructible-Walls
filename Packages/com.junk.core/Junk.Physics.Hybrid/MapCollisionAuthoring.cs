using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Physics.Hybrid
{
    public class MapCollisionAuthoring: MonoBehaviour
    {
        public class MapCollisionBaker : Baker<MapCollisionAuthoring>
        {
            public override void Bake(MapCollisionAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace | TransformUsageFlags.Renderable);
                AddComponent<MapStatic>(entity);
                
                if(authoring.transform.localScale.x != 1 || authoring.transform.localScale.y != 1 || authoring.transform.localScale.z != 1)
                    Debug.LogWarning("MapCollisionAuthoring: Scale is not 1,1,1. This may cause issues. Please bake with a scale of 1,1,1");
            }
        }
    }
    
    //[TemporaryBakingType]
    public struct MapStatic : IComponentData
    {
        
    }
    
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public unsafe partial struct PhysicsEnvironmentStaticBakingSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var physicsCollider in SystemAPI.Query<RefRW<PhysicsCollider>>().WithAll<MapStatic>().WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities))
            {
                //var filter = physicsCollider.ValueRW.Value.Value.GetCollisionFilter();

                //filter.BelongsTo = Layers.EnvironmentStatic();
                //filter.CollidesWith = Layers.WorldCollisionMatrix();
                
                //physicsCollider.ValueRW.Value.Value.SetCollisionFilter(filter);
            }
        }
    }
}