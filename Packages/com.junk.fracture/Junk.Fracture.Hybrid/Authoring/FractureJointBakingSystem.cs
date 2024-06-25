using System.Collections.Generic;
using System.Linq;
using Junk.Math;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
// ReSharper disable Unity.Entities.MustBeSurroundedWithRefRwRo
/*
 * make me a method that creates a list of entities:
I have a list called FractureGraph which contains every fracturable entity
Each fracturable entity has a list of entities that are connected to it called EntityAnchors
Every entity is connected to at least one other entity, and all the entities are connected in a chain.
I need a list of lists for each fracturable entity that is the shortest path to every other entity in the graph.

 */
namespace Junk.Fracture.Hybrid
{
    [DisableAutoCreation]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    [UpdateAfter(typeof(FractureNeighborBakingSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial struct FractureJointBakingSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<EntityAnchor>();
            builder.WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab);
            query = builder.Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (root, graph, entity) in SystemAPI.Query<RefRW<FractureRoot>, DynamicBuffer<FractureGraph>>()
                         .WithOptions(EntityQueryOptions.IncludePrefab).WithEntityAccess())
            {
                // Setup go proxies
                var entities = graph.AsNativeArray();
                var shortestPaths = FindAllShortestPaths(ref state, entities);
            }

            ecb.Playback(state.EntityManager);
        }
        
        
        public static Dictionary<Entity, Dictionary<Entity, List<Entity>>> FindAllShortestPaths(ref SystemState state, NativeArray<FractureGraph> fracturableEntities)
        {
            var shortestPaths = new Dictionary<Entity, Dictionary<Entity, List<Entity>>>();

            // Initialize shortest paths dictionary
            foreach (Entity entity in fracturableEntities)
            {
                // Initialize dictionary for entity
                shortestPaths[entity] = new Dictionary<Entity, List<Entity>>();
                
                foreach (var otherEntity in fracturableEntities)
                {
                    var entityAnchors = state.EntityManager.GetBuffer<EntityAnchor>(entity).ToList();
                    
                    if (entity == otherEntity)
                    {
                        shortestPaths[entity][otherEntity] = new List<Entity> { entity };
                    }
                    else if (entityAnchors.Any(anchor => anchor.ConnectedEntity == otherEntity))
                    {
                        shortestPaths[entity][otherEntity] = new List<Entity> { entity, otherEntity };
                    }
                    else
                    {
                        shortestPaths[entity][otherEntity] = null;
                    }
                }
            }

            // Floyd-Warshall algorithm
            foreach (var k in fracturableEntities)
            {
                foreach (var i in fracturableEntities)
                {
                    foreach (var j in fracturableEntities)
                    {
                        if (shortestPaths[i][k] != null && shortestPaths[k][j] != null)
                        {
                            var newPath = new List<Entity>(shortestPaths[i][k]);
                            newPath.AddRange(shortestPaths[k][j].Skip(1));

                            if (shortestPaths[i][j] == null || newPath.Count < shortestPaths[i][j].Count)
                            {
                                shortestPaths[i][j] = newPath;
                            }
                        }
                    }
                }
            }

            return shortestPaths;
        }
    }
}