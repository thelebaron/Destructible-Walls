using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace Junk.Entities
{
    /// <summary>
    /// This component signifies the pool entity is attached and active, and should be updated.
    /// </summary>
    public struct PoolEntityReference : IComponentData, IEnableableComponent
    {
        public Entity Entity;
    }
    
    public struct ReturnToPool : IComponentData, IEnableableComponent
    {
    }
    
    public struct GetFromPool : IComponentData, IEnableableComponent
    {
    }
    
    public class LightRef : IComponentData, IDisposable, ICloneable
    {
        public Light Light;

        public void Dispose()
        {
            UnityEngine.Object.Destroy(Light.gameObject);
        }

        public object Clone()
        {
            return new LightRef { Light = UnityEngine.Object.Instantiate(Light) };
        }
    }
    
    [DisableAutoCreation]
    [RequireMatchingQueriesForUpdate]
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial class LightPoolSystem : SystemBase
    {
        private EntityArchetype lightArchetype;
        
        protected override void OnCreate()
        {
            lightArchetype = EntityManager.CreateArchetype(typeof(LocalToWorld), typeof(LocalTransform), typeof(PoolEntityReference), typeof(Light), typeof(GetFromPool), typeof(ReturnToPool));
            
            for (int i = 0; i < 5; i++)
            {
                CreateLightEntity();
            }
        }
        
        private void CreateLightEntity()
        {
            var entity = EntityManager.CreateEntity(lightArchetype);
            //Debug.Log("Pooled Light Object Created");
            SystemAPI.SetComponentEnabled<PoolEntityReference>(entity, false);
            EntityManager.SetComponentEnabled<ReturnToPool>(entity, false);
            EntityManager.SetComponentEnabled<GetFromPool>(entity, false);
            
            var go = new GameObject("Pooled Light Object");
            go.hideFlags = HideFlags.DontSave;
            var light = go.AddComponent<Light>();
            light.intensity = 0;
            EntityManager.AddComponentObject(entity, light);
            EntityManager.AddComponentObject(entity, light.transform);
            EntityManager.AddComponentObject(entity, new LightRef { Light = light });
        }
        
        protected override void OnUpdate()
        {
            foreach (var (localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<ReturnToPool>().WithAll<PoolEntityReference>().WithAll<Light>().WithEntityAccess())
            {
                var light = EntityManager.GetComponentObject<Light>(entity);
                light.intensity = 0;
                SystemAPI.SetComponentEnabled<PoolEntityReference>(entity, false);
                SystemAPI.SetComponentEnabled<ReturnToPool>(entity, false);
            }
            
            foreach (var (localTransform, unmanagedLight, reference, entity) in SystemAPI.Query<RefRW<LocalToWorld>, RefRW<UnmanagedLightData>, RefRW<UnmanagedLightReference>>().WithEntityAccess())
            {
                var light = EntityManager.GetComponentObject<Light>(reference.ValueRO.Entity);
                light.intensity      = unmanagedLight.ValueRO.Intensity;
                light.color          = unmanagedLight.ValueRO.Color;
                light.range          = unmanagedLight.ValueRO.Range;
                light.shadowStrength = unmanagedLight.ValueRO.ShadowStrength;
                light.enabled        = unmanagedLight.ValueRO.Enabled;
                light.transform.position = localTransform.ValueRO.Position;
                light.transform.rotation = localTransform.ValueRO.Rotation;
            }
            /*
            var lightTransformQuery = SystemAPI.QueryBuilder()
                .WithAllRW<Transform>().WithAll<PoolEntityReference, LocalTransform>()
                .Build();
            
            var localTransformAccess = lightTransformQuery.GetTransformAccessArray();
            var lightTransforms = lightTransformQuery.ToComponentDataListAsync<LocalTransform>(WorldUpdateAllocator, out var dependency);
            Dependency = JobHandle.CombineDependencies(Dependency, dependency);
            Dependency = new CopyLocalTransformsJob
            {
                LocalTransforms = lightTransforms.AsDeferredJobArray()
            }
            .Schedule(localTransformAccess, this.Dependency);*/
        }
        
        [BurstCompile]
        private struct CopyLocalTransformsJob : IJobParallelForTransform
        {
            [ReadOnly]
            public NativeArray<LocalTransform> LocalTransforms;

            public void Execute(int index, TransformAccess transform)
            {
                var value = this.LocalTransforms[index];
                transform.position = value.Position;
                transform.rotation = value.Rotation;
            }
        }
    }
}