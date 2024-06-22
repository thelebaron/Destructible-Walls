using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Junk.Entities
{
    [DisableAutoCreation]
    public partial struct LightTransformSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
             // this errors about UnmanagedLightData not being correctly assigned to the Dependency property
            state.Dependency = new LightUpdateLocalTransformJob
            {
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
            }.Schedule(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct LightUpdateLocalTransformJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            
            public void Execute(Entity entity, in UnmanagedLightReference reference, in LocalToWorld localToWorld)
            {
                LocalTransformLookup[reference.Entity] = LocalTransform.FromPositionRotation(localToWorld.Position, localToWorld.Rotation);
            }
        }
    }
}