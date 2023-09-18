using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Physics
{
    // If beyond threshold, then disable physics
    public struct PhysicsStreamingConfig : IComponentData
    {
        public float DistanceForStreamingIn;
        public float DistanceForStreamingOut;
    }
    
    //[TemporaryBakingType]
    public struct PhysicsPositionHash : IComponentData
    {
        public int HashValue;
    }
    
    public struct PhysicsSectionTag : IComponentData
    {
        public Entity Value;
    }
    
    public struct PhysicsSectionBoundingVolume : IComponentData
    {
        /// <summary>
        /// Bounding volume
        /// </summary>
        public MinMaxAABB BoundingVolume;
        public int  HashValue;
        public bool DebugToggle;
    }
    
    public struct PhysicsSectionEntities : IBufferElementData
    {
        public Entity Value;
        
        public static implicit operator Entity(PhysicsSectionEntities e) { return e.Value; }
        public static implicit operator PhysicsSectionEntities(Entity e) { return new PhysicsSectionEntities { Value = e }; }
    }
    
    [DisableAutoCreation]
    [BurstCompile]
    //would be nice for this to be a baking system not runtime
    [RequireMatchingQueriesForUpdate]
    public partial struct PhysicsSectionSystem : ISystem
    {
        private EndInitializationEntityCommandBufferSystem.Singleton entityCommandBufferSystem;
        private NativeParallelMultiHashMap<int, Entity>                      spatialHashMap;

        [BurstCompile]
        public void OnCreate(ref  SystemState state)
        {
            state.RequireForUpdate<PhysicsStreamingConfig>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            var cameraEntity           = SystemAPI.GetSingletonEntity<PhysicsStreamingConfig>();
            var physicsStreamingConfig = SystemAPI.GetSingleton<PhysicsStreamingConfig>();
            var distanceStreamingIn    = physicsStreamingConfig.DistanceForStreamingIn;
            var distanceStreamingOut   = physicsStreamingConfig.DistanceForStreamingOut;
            var cameraLocal            = SystemAPI.GetComponent<LocalToWorld>(cameraEntity);
            var cameraPos              = cameraLocal.Position;
            
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            // dynamicbuffer
             foreach (var (/*localToWorld, */section, elements) in SystemAPI.Query<RefRW<PhysicsSectionBoundingVolume>, DynamicBuffer<PhysicsSectionEntities>>())
             {
                 AABB aabb           = section.ValueRO.BoundingVolume;
                 var  distance       = math.distance(aabb.Center, cameraPos);
                 AABB boundingVolume = section.ValueRO.BoundingVolume;
                 var  distanceSq     = boundingVolume.DistanceSq(cameraPos);

                 
                 
                 if (distanceSq > distanceStreamingOut)
                 {
                     foreach (var element in elements) 
                     {
                         if (state.EntityManager.HasComponent<Disabled>(element.Value))
                             continue;

                         commandBuffer.AddComponent<Disabled>(element.Value);
                         
                         if (state.EntityManager.HasComponent<Child>(element.Value))
                         {
                             var childBuffer = state.EntityManager.GetBuffer<Child>(element.Value);
                             var childEntity = childBuffer[0].Value;
                             if(!state.EntityManager.HasComponent<Disabled>(element.Value))
                                 commandBuffer.AddComponent<Disabled>(childBuffer[0].Value);
                         }
                     }
                 }
                 
                 if (distanceSq < distanceStreamingIn)
                 {
                     foreach (var element in elements) 
                     {
                         if (!state.EntityManager.HasComponent<Disabled>(element.Value))
                             continue;
                         
                         commandBuffer.RemoveComponent<Disabled>(element.Value);
                         if (state.EntityManager.HasComponent<Child>(element.Value))
                         {
                             var childBuffer = state.EntityManager.GetBuffer<Child>(element.Value);
                             var childEntity = childBuffer[0].Value;
                             if(state.EntityManager.HasComponent<Disabled>(element.Value))
                                 commandBuffer.RemoveComponent<Disabled>(childBuffer[0].Value);
                         }
                     }
                 }
             }
            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }
    }


    
    
}