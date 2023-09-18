using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Junk.Physics
{
    // If beyond threshold, then disable physics
    public struct PhysicsLOD : IComponentData
    {
        public float DistanceForStreamingIn;
        public float DistanceForStreamingOut;
    }

    [DisableAutoCreation]
    public partial struct PhysicsLODSystem : ISystem
    {
        private EndInitializationEntityCommandBufferSystem.Singleton entityCommandBufferSystem;
        private NativeParallelMultiHashMap<int, Entity>              spatialHashMap;

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var configQuery = SystemAPI.QueryBuilder()
                .WithAll<PhysicsStreamingConfig>()
                .WithAll<LocalToWorld>().Build();
            var physicsStreamInQuery = SystemAPI.QueryBuilder()
                .WithNone<PhysicsWorldIndex>()
                .WithAll<PhysicsLOD>()
                .WithAll<LocalToWorld>()
                .Build();
            var physicsStreamOutQuery = SystemAPI.QueryBuilder()
                .WithAll<PhysicsWorldIndex>()
                .WithAll<PhysicsLOD>()
                .WithAll<LocalToWorld>()
                .Build();

            if (configQuery.CalculateEntityCount().Equals(0))
                return;

            var streamingLogicConfig = configQuery.GetSingleton<PhysicsStreamingConfig>();
            var cameraPosition       = configQuery.GetSingleton<LocalToWorld>().Position;
            var commandBuffer        = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            var disableList          = new NativeList<Entity>(Allocator.TempJob);
            var enableList           = new NativeList<Entity>(Allocator.TempJob);
            var hashMap              = new NativeParallelMultiHashMap<int, Entity>(physicsStreamOutQuery.CalculateEntityCount(), Allocator.TempJob);

            state.Dependency = new PhysicsStreamInJob
            {
                MaxDistanceSquared = streamingLogicConfig.DistanceForStreamingOut,
                CameraPosition     = cameraPosition,
                EnableList         = enableList
            }.Schedule(physicsStreamInQuery, state.Dependency);

            state.Dependency = new PhysicsStreamOutJob
            {
                MaxDistanceSquared = streamingLogicConfig.DistanceForStreamingOut,
                CameraPosition     = cameraPosition,
                DisableList        = disableList
            }.Schedule(physicsStreamOutQuery, state.Dependency);

            state.Dependency = new PhysicsChangeBufferJob
            {
                CommandBuffer = commandBuffer,
                EnableArray   = disableList,
                DisableArray  = enableList
            }.Schedule(state.Dependency);

            disableList.Dispose(state.Dependency);
            enableList.Dispose(state.Dependency);
            hashMap.Dispose(state.Dependency);
        }

        [BurstCompile]
        private partial struct PhysicsLODHashJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, Entity>.ParallelWriter HashMap;
            public float                                                  MaxDistanceSquared;
            public float3                                                 CameraPosition;

            public void Execute(Entity entity, LocalToWorld localToWorld)
            {
                var position = localToWorld.Position;
                var hash     = HashedPosition(position, 50); //(int)math.hash(new int3(math.floor(localToWorld.Position / cellRadius)));
                HashMap.Add(hash, entity); // was entityInQueryIndex
            }

            private static int HashedPosition(float3 position, float cellRadius)
            {
                var hashPos = (int)math.hash(new int3(math.floor(position / cellRadius)));
                return hashPos;
            }
        }

        [BurstCompile]
        private partial struct PhysicsCheckPositionHashJob : IJobEntity
        {
            public float3                                  CameraPosition;
            public NativeParallelMultiHashMap<int, Entity> HashMap;

            public void Execute(Entity entity, LocalToWorld localToWorld)
            {
                var positon      = CameraPosition;
                var hash         = (int)math.hash(new int3(math.floor(positon / 50)));
                var containsHash = HashMap.ContainsKey(hash);
            }
        }


        [BurstCompile]
        private partial struct PhysicsStreamOutJob : IJobEntity
        {
            public                                       float              MaxDistanceSquared;
            public                                       float3             CameraPosition;
            [NativeDisableParallelForRestriction] public NativeList<Entity> DisableList;

            public void Execute(Entity entity, LocalToWorld localToWorld)
            {
                var position         = localToWorld.Position;
                var cameraDistanceSq = math.lengthsq(CameraPosition);
                var entityDistanceSq = math.lengthsq(position);

                // if distance between cameraDistanceSq and entityDistanceSq is greater than MaxDistanceSquared, then disable physics
                if (math.abs(cameraDistanceSq - entityDistanceSq) > MaxDistanceSquared)
                {
                    //Debug.Log($"Disabling physics for {entity}");
                    //DisableList.Add(entity);
                }
            }
        }


        [BurstCompile]
        private partial struct PhysicsStreamInJob : IJobEntity
        {
            public float  MaxDistanceSquared;
            public float3 CameraPosition;

            [NativeDisableContainerSafetyRestriction]
            public NativeList<Entity> EnableList;

            public void Execute(Entity entity, LocalToWorld localToWorld)
            {
                var position         = localToWorld.Position;
                var cameraDistanceSq = math.lengthsq(CameraPosition);
                var entityDistanceSq = math.lengthsq(position);

                // if distance between cameraDistanceSq and entityDistanceSq is greater than MaxDistanceSquared, then disable physics
                if (math.abs(cameraDistanceSq - entityDistanceSq) < MaxDistanceSquared)
                {
                    //Debug.Log($"Enabling physics for {entity}");
                    //EnableList.Add(entity);
                }
            }
        }


        [BurstCompile]
        private struct PhysicsChangeBufferJob : IJob
        {
            public            EntityCommandBuffer CommandBuffer;
            [ReadOnly] public NativeList<Entity>  EnableArray;
            [ReadOnly] public NativeList<Entity>  DisableArray;

            public void Execute()
            {
                for (var i = 0; i < EnableArray.Length; i++)
                {
                    //Debug.Log($"AddSharedComponent physics for {EnableArray[i]}");
                    //CommandBuffer.AddSharedComponent<PhysicsWorldIndex>(EnableArray[i], new PhysicsWorldIndex());
                }

                for (var i = 0; i < DisableArray.Length; i++)
                {
                    //Debug.Log($"RemoveComponent physics for {DisableArray[i]}");
                    //CommandBuffer.RemoveComponent<PhysicsWorldIndex>(DisableArray[i]);
                }
            }
        }
    }
}