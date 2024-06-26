using Junk.Math;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Junk.Fracture
{
    public partial struct BreakObjectSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingletonRW<BeginInitializationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            
            // ReSharper disable once Unity.Entities.MustBeSurroundedWithRefRwRo
            foreach (var (breakable, localToWorld, entity) in SystemAPI.Query<RefRO<Breakable>, RefRO<LocalToWorld>>().WithEntityAccess())
            {
                var prefab = breakable.ValueRO.Prefab;
                var fractureBuffer = state.EntityManager.GetBuffer<FractureChild>(prefab);
                
                foreach (var fractureChild in fractureBuffer)
                {
                    var child = fractureChild.Child;
                    
                    var fractureEntity = ecb.Instantiate(child);
                    // get scale from 4x4
                    var scale = localToWorld.ValueRO.Value.GetScale();
                    var tr    = LocalTransform.FromPositionRotationScale(localToWorld.ValueRO.Position, localToWorld.ValueRO.Rotation, scale);
                    
                    ecb.SetComponent(fractureEntity, tr);
                    
                }
                ecb.DestroyEntity(entity);
            }
            
            
            
            
            
            // ReSharper disable once Unity.Entities.MustBeSurroundedWithRefRwRo
            foreach (var (fractureChildren, localToWorld, localTransform, entity) in SystemAPI.Query<DynamicBuffer<FractureChild>, RefRO<LocalToWorld>, RefRO<LocalTransform>>().WithAll<IsFractured>().WithEntityAccess())
            {
                foreach (var fractureChild in fractureChildren)
                {
                    var child = fractureChild.Child;
                    
                    var fractureEntity = ecb.Instantiate(child);
                    // get scale from 4x4
                    var scale = localToWorld.ValueRO.Value.GetScale();
                    var tr    = LocalTransform.FromPositionRotationScale(localTransform.ValueRO.Position, localTransform.ValueRO.Rotation, scale);
                    
                    ecb.SetComponent(fractureEntity, tr);
                    
                }
                ecb.DestroyEntity(entity);
            }
        }
    }
}