using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Entities
{
    public class TetherSingletonAuthoring : MonoBehaviour
    {
        public class TetherSingletonAuthoringBaker : Baker<TetherSingletonAuthoring>
        {
            public override void Bake(TetherSingletonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TetherSingleton());
                AddBuffer<WorldTetherPoint>(entity);
            }
        }
    }


    public partial struct TetherSingletonBakingSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TetherSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var worldTetherEntity = SystemAPI.GetSingletonEntity<TetherSingleton>();
            var worldTetherPoints = SystemAPI.GetBuffer<WorldTetherPoint>(worldTetherEntity);

            if (worldTetherPoints.Length.Equals(0))
            {
                foreach (var (localToWorld, tetherData, tetherSamplePoints, entity) in SystemAPI
                             .Query<RefRO<LocalToWorld>, RefRO<TetherData>, DynamicBuffer<TetherSamplePoint>>()
                             .WithEntityAccess())
                {
                    var element = new WorldTetherPoint
                    {
                        Entity     = entity,
                        Position   = localToWorld.ValueRO.Position,
                        Visibility = 0
                    };
                    worldTetherPoints.Add(element);
                }
            }
        }

        public void OnStartRunning(ref SystemState state)
        {
            var worldTetherEntity = SystemAPI.GetSingletonEntity<TetherSingleton>();
            var worldTetherPoints = SystemAPI.GetBuffer<WorldTetherPoint>(worldTetherEntity);

            if (worldTetherPoints.Length.Equals(0))
            {
                foreach (var (localToWorld, tetherData, tetherSamplePoints, entity) in SystemAPI
                             .Query<RefRO<LocalToWorld>, RefRO<TetherData>, DynamicBuffer<TetherSamplePoint>>()
                             .WithEntityAccess())
                {
                    var element = new WorldTetherPoint
                    {
                        Entity     = entity,
                        Position   = localToWorld.ValueRO.Position,
                        Visibility = 0
                    };
                    worldTetherPoints.Add(element);
                }
            }
            
        }

        public void OnStopRunning(ref  SystemState state)
        {
            
        }
    }
}