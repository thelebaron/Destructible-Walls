using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Junk.Entities
{
 
    [MaterialProperty("_DissolveAmount")]
    public struct MaterialPropertyDissolve : IComponentData, IEnableableComponent
    {
        public float Value;
    }

    [InternalBufferCapacity(0)]
    public struct LinkedDissolveGroup : IBufferElementData, IEnableableComponent
    {
        public Entity Value;
    }
    
    [InternalBufferCapacity(1)]
    public struct RenderEntityGroup : IBufferElementData
    {
        public Entity Value;
    }
    
    public struct DestroyAfterDissolve : IComponentData
    {
    }
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial struct SetHumanRagdollSystem : ISystem
    {
        
       /* [BurstCompile]
        [WithDisabled(typeof(LinkedDissolveGroup))]
        [WithAll(typeof(Ragdoll))]
        private partial struct EnableCharacterDissolveJob : IJobEntity
        {
            public BufferLookup<LinkedDissolveGroup> LinkedBufferLookup;
            public void Execute(Entity entity, in Timer timer)
            {
                // unsure if needed but quick overlysafe check
                if (LinkedBufferLookup.HasBuffer(entity))
                {
                    int seconds = (int)timer.Value % 60;
                
                    if(seconds >= 2)
                    {
                        LinkedBufferLookup.SetBufferEnabled(entity, true);
                    }
                }
            }
        }    */ 
        
        [BurstCompile]
        [WithChangeFilter(typeof(LinkedDissolveGroup))]
        private partial struct LinkedDissolveChangeFilterJob : IJobEntity
        {
            public ComponentLookup<MaterialPropertyDissolve> MaterialPropertyDissolveLookup;
            
            public void Execute(Entity entity, DynamicBuffer<LinkedDissolveGroup> linkedDissolveGroup)
            {
                for (int i = 0; i < linkedDissolveGroup.Length; i++)
                {
                    var materialEntity = linkedDissolveGroup[i].Value;
                    
                    var isEnabled = MaterialPropertyDissolveLookup.IsComponentEnabled(materialEntity);
                    MaterialPropertyDissolveLookup.SetComponentEnabled(materialEntity, !isEnabled);
                }
            }
        }
        
        [BurstCompile]
        private partial struct MaterialDissolveJob : IJobEntity
        {
            public            float DeltaTime;
            public void Execute(Entity entity, ref MaterialPropertyDissolve materialPropertyDissolve)
            {
                materialPropertyDissolve.Value = math.lerp(materialPropertyDissolve.Value, 0, DeltaTime);
            }
        }
        
        [BurstCompile]
        [WithAll(typeof(DestroyAfterDissolve))]
        private partial struct DissolveFadeJob : IJobEntity
        {
            public EntityCommandBuffer                       EntityCommandBuffer;
            [ReadOnly] public ComponentLookup<MaterialPropertyDissolve> MaterialPropertyDissolveLookup;
            
            public void Execute(Entity entity, DynamicBuffer<LinkedDissolveGroup> linkedDissolveGroup)
            {
                for (int i = 0; i < linkedDissolveGroup.Length; i++)
                {
                    var linkedEntity             = linkedDissolveGroup[i].Value;
                    var materialPropertyDissolve = MaterialPropertyDissolveLookup[linkedEntity];
                    
                    if(materialPropertyDissolve.Value <= 0)
                    {
                        EntityCommandBuffer.DestroyEntity(entity);
                    }
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new DissolveFadeJob
            {
                EntityCommandBuffer = SystemAPI.GetSingletonRW<DestroyCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                MaterialPropertyDissolveLookup = SystemAPI.GetComponentLookup<MaterialPropertyDissolve>(true)
            }.Schedule(state.Dependency);

            state.Dependency = new LinkedDissolveChangeFilterJob
            {
                MaterialPropertyDissolveLookup = SystemAPI.GetComponentLookup<MaterialPropertyDissolve>()
            }.Schedule(state.Dependency);

            // todo move to separate system
            state.Dependency = new MaterialDissolveJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.Schedule(state.Dependency);
        }
    }
}