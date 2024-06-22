using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    public struct SmokeCommand : IComponentData
    {
        public float3 Position;
    }

    public struct SparkCommand : IComponentData
    {
        public float3 Position;
        public float3 Normal;
        public float  Size;
        public int    Count;
        public float  Speed;
        public float3 Gravity;
        public float2 LifeMinMax;
    }
    
    public struct BloodsprayCommand : IComponentData
    {
        public int    Type;
        public float3 Normal;
        public float3 Position;
        public float3 Angle;// for vfxgraph, it expects a world space euler angle
        public float3 ImpactDirectionNormalized; // the direction of the bullet
    }
    
    public static class VFXUtility
    {
        /// <summary>
        /// Burst compatible way of creating an archetype for entity instantiation
        /// </summary>
        private static EntityArchetype SmokeCommandEntityArchetype(ref SystemState state)
        {
            // Create command entities for visual effect command use
            var smokeCommandTypes = new NativeArray<ComponentType>(3, Allocator.Temp);
            smokeCommandTypes[0] = ComponentType.ReadWrite<SmokeCommand>();
            smokeCommandTypes[1] = ComponentType.ReadWrite<Destroy>();
            smokeCommandTypes[2] = ComponentType.ReadWrite<Prefab>();
            return state.EntityManager.CreateArchetype(smokeCommandTypes);
        }

        /// <summary>
        /// Burst compatible way of creating an archetype for entity instantiation
        /// </summary>
        private static EntityArchetype SparkCommandEntityArchetype(ref SystemState state)
        {
            var sparkCommandTypes = new NativeArray<ComponentType>(3, Allocator.Temp);
            sparkCommandTypes[0] = ComponentType.ReadWrite<SparkCommand>();
            sparkCommandTypes[1] = ComponentType.ReadWrite<Destroy>();
            sparkCommandTypes[2] = ComponentType.ReadWrite<Prefab>();
            return state.EntityManager.CreateArchetype(sparkCommandTypes);
        }
        
        /// <summary>
        /// Burst compatible way of creating an archetype for entity instantiation
        /// </summary>
        private static EntityArchetype BloodsprayCommandEntityArchetype(ref SystemState state)
        {
            var sparkCommandTypes = new NativeArray<ComponentType>(3, Allocator.Temp);
            sparkCommandTypes[0] = ComponentType.ReadWrite<BloodsprayCommand>();
            sparkCommandTypes[1] = ComponentType.ReadWrite<Destroy>();
            sparkCommandTypes[2] = ComponentType.ReadWrite<Prefab>();
            return state.EntityManager.CreateArchetype(sparkCommandTypes);
        }
        
        /// <summary>
        /// A smoke vfx command entity, for consumption by the vfx bridge system to spawn a smoke vfx
        /// </summary>
        public static Entity CreateSmokeCommandPrefab(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity(SmokeCommandEntityArchetype(ref state));
        #if UNITY_EDITOR
            state.EntityManager.SetName(entity, "Smoke Command Entity");
        #endif
            state.EntityManager.SetComponentEnabled<Destroy>(entity, false);
            return entity;
        }
        
        /// <summary>
        /// A spark vfx command entity, for consumption by the vfx bridge system to spawn a spark vfx
        /// </summary>
        public static Entity CreateSparkCommandPrefab(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity(SparkCommandEntityArchetype(ref state));
#if UNITY_EDITOR
            state.EntityManager.SetName(entity, "Smoke Command Entity");
#endif
            state.EntityManager.SetComponentEnabled<Destroy>(entity, false);
            return entity;
        }
                
        /// <summary>
        /// A blood vfx command entity, for consumption by the vfx bridge system to spawn a spark vfx
        /// </summary>
        public static Entity CreateBloodCommandPrefab(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity(BloodsprayCommandEntityArchetype(ref state));
#if UNITY_EDITOR
            state.EntityManager.SetName(entity, "Blood Command Entity");
#endif
            state.EntityManager.SetComponentEnabled<Destroy>(entity, false);
            return entity;
        }
    }
}