using Junk.Entities;
using Junk.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
// ReSharper disable Unity.Entities.MustBeSurroundedWithRefRwRo

namespace Junk.Entities
{
    [DisableAutoCreation]
    public partial struct TetherSystem : ISystem
    {
        private EntityQuery query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Player>();
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TetherSamplePoint, TetherData>()
                .WithAll<LocalToWorld>()
                .Build(ref state);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (tetherSingleton, tetherPoints, entity) in SystemAPI.Query<RefRO<TetherSingleton>, DynamicBuffer<WorldTetherPoint>>().WithEntityAccess())
            {
                var buffer = tetherPoints.AsNativeArray();
                for (int i = 0; i < buffer.Length; i++)
                {
                    var tetherPoint = buffer[i];
                    tetherPoint.Visibility = SystemAPI.GetComponent<TetherData>(tetherPoint.Entity).VisibileScore;
                    tetherPoint.CanSeePlayer = SystemAPI.GetComponent<TetherData>(tetherPoint.Entity).PlayerVisible;
                    
                    buffer[i] = tetherPoint;
                }
            }
            
            state.Dependency = new CalculateTetherVisJob
            {
                DeltaTime            = SystemAPI.Time.DeltaTime,
                Player               = SystemAPI.GetSingletonEntity<Player>(),
                LocalToWorldLookupRO = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                PhysicsWorld         = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld,
                EntityTypeHandle     = SystemAPI.GetEntityTypeHandle(),
                TetherDataHandle     = SystemAPI.GetComponentTypeHandle<TetherData>(),
                TetherPointHandle    = SystemAPI.GetBufferTypeHandle<TetherSamplePoint>()
            }.Schedule(query, state.Dependency);


        }
        
        [BurstCompile]
        private struct CalculateTetherVisJob : IJobChunk
        {
            public            Entity                            Player;
            [ReadOnly] public ComponentLookup<LocalToWorld>     LocalToWorldLookupRO;
            public            float                             DeltaTime;
            [ReadOnly] public PhysicsWorld                      PhysicsWorld;
            public            EntityTypeHandle                  EntityTypeHandle;
            public            ComponentTypeHandle<TetherData>   TetherDataHandle;
            public            BufferTypeHandle<TetherSamplePoint>     TetherPointHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var playerPosition = LocalToWorldLookupRO[Player].Position;
                
                var entities = chunk.GetNativeArray(EntityTypeHandle);
                var tetherDatas = chunk.GetNativeArray(ref TetherDataHandle);
                var tetherPoints = chunk.GetBufferAccessor(ref TetherPointHandle);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var entity = entities[i];
                    var tetherData = tetherDatas[i];
                    tetherData.Elapsed       += DeltaTime;

                    if (tetherData.Elapsed < tetherData.Rate)
                    {
                        tetherDatas[i] = tetherData;
                        continue;
                    }
                    
                    tetherData.PlayerVisible = false;
                    
                    var visibleCount = 0;
                    var invalidCount = 0;
                    var tetherPointBuffer = tetherPoints[i];
                    for (int j = 0; j < tetherPointBuffer.Length; j++)
                    {
                        var tetherPoint = tetherPointBuffer[j];
                        var raycastInput = new RaycastInput
                        {
                            Start =/* localToWorld.Position +*/ tetherPoint.Point,
                            End   = (/*localToWorld.Position +*/ tetherPoint.Point) + (maths.down * 2f),
                            Filter = CollisionFilter.Default
                            
                        };
                        var filter = new CollisionFilter
                        {
                            BelongsTo    = 1 << 1,
                            CollidesWith = 1 << 0,
                            GroupIndex   = 0
                        };
                        // draw a ray
                        var terrainHit = PhysicsWorld.CastRay(raycastInput, out var raycastHit);
                        tetherPoint.Valid        = terrainHit;
                        
                        
                        //Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.red, 0.1f);
                        
                        if (tetherPoint.Valid)
                        {
                            Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.green, 0.02f);
                            raycastInput = new RaycastInput
                            {
                                Start  = tetherPoint.Point,
                                End    = (playerPosition) + maths.up * 1.75f,
                                Filter = CollisionFilter.Default
                            };
                            // draw a ray
                            
                            var playerHit = PhysicsWorld.CastRay(raycastInput, out var playerRaycastHit);
                            //Debug.Log(playerRaycastHit.Entity);
                            if (playerRaycastHit.Entity.Equals(Player))
                            {
                                //Debug.Log(playerRaycastHit.Entity.Equals(Player) + " " + playerRaycastHit.Entity + " " + Player);
                                tetherPoint.CanSeePlayer = true;
                                tetherData.PlayerVisible = true;
                                Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.green, 0.02f);
                                tetherPointBuffer[j] = tetherPoint;
                            }
                            else
                            {
                                tetherPoint.CanSeePlayer = false;
                                //Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.red, 1f);
                                tetherPointBuffer[j] = tetherPoint;
                            }
                            visibleCount += tetherPoint.CanSeePlayer ? 1 : 0;
                        }
                        else
                        {
                            invalidCount++;
                        }
                        tetherPointBuffer[j] = tetherPoint;
                    }
                    
                    var distanceToPlayer = math.distance(playerPosition, LocalToWorldLookupRO[entity].Position);
                    
                    tetherData.VisibileScore = visibleCount / (float) (tetherPointBuffer.Length - invalidCount);
                        // fix nan
                        tetherData.VisibileScore = float.IsNaN(tetherData.VisibileScore) ? 0 : tetherData.VisibileScore;
                    
                    tetherData.DistanceToPlayer  = distanceToPlayer;
                    tetherDatas[i]               = tetherData;
                }
            }
        }
    }
}