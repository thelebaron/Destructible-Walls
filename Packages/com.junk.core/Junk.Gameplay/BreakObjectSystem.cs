using Junk.Entities;
using Junk.Health;
using Junk.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Fracture
{
    //[DisableAutoCreation]
    public partial struct FractureEntitySystem : ISystem
    {
        public partial struct BreakableJob : IJobEntity
        {
            public EntityCommandBuffer         CommandBuffer;
            public BufferLookup<FractureChild> FractureChildLookup;
            
            public void Execute(Entity entity, in Breakable breakable, in HealthData health, in LocalToWorld localToWorld)
            {
                if(health.Current>0)
                    return;
                
                //var breakable      = BreakableLookup[entity];
                var prefab         = breakable.Prefab;
                var fractureBuffer = FractureChildLookup[prefab];
                
                foreach (var fractureChild in fractureBuffer)
                {
                    var child = fractureChild.Child;
                    
                    var fractureEntity = CommandBuffer.Instantiate(child);
                    // get scale from 4x4
                    var scale = localToWorld.Value.GetScale();
                    var tr    = LocalTransform.FromPositionRotationScale(localToWorld.Position, localToWorld.Rotation, scale);
                    
                    CommandBuffer.SetComponent(fractureEntity, tr);
                    
                }
                CommandBuffer.SetComponentEnabled<Destroy>(entity, true);
            }
        }
        
        [WithAll(typeof(Breakable))]
        [WithDisabled(typeof(Destroy))]
        public partial struct DestroyBreakableJob : IJobEntity
        {
            public EntityCommandBuffer CommandBuffer;
            
            public void Execute(Entity entity, in Breakable breakable)
            {
                CommandBuffer.SetComponentEnabled<Destroy>(entity, true);
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new BreakableJob
            {
                CommandBuffer       = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                FractureChildLookup = SystemAPI.GetBufferLookup<FractureChild>(true)
            }.Schedule(state.Dependency);
        }
    }
}