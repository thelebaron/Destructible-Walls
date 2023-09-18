using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Junk.Physics
{
    //would be nice for this to be a baking system not runtime
    [RequireMatchingQueriesForUpdate]
    public partial struct PhysicsStreamingSectionInitializationSystem : ISystem
    {
        private EndInitializationEntityCommandBufferSystem.Singleton entityCommandBufferSystem;
        private NativeParallelMultiHashMap<int, Entity>                      spatialHashMap;
        private EntityQuery                                          lodQuery;

        public void OnCreate(ref  SystemState state)
        {
            lodQuery = SystemAPI.QueryBuilder()
                .WithAll<PhysicsLOD>()
                .WithAll<PhysicsPositionHash>()
                .WithAll<LocalToWorld>().Build();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref  SystemState state)
        {
            
            /* dynamicbuffer
             foreach (var elements in SystemAPI.Query<DynamicBuffer<PhysicsSection>>()){
                foreach (var element in elements) {
                    //Debug.Log(element);
                }
            }*/
            
            var hashmap = new NativeParallelMultiHashMap<int, Entity>(lodQuery.CalculateEntityCount(), Allocator.TempJob);
            //var entity  = state.EntityManager.CreateEntity();
            //state.EntityManager.AddComponentData(entity, new PhysicsSceneSection {HashValue = 1});
            
            //var vehicles = CollectionHelper.CreateNativeArray<Entity>(lodQuery.CalculateEntityCount(), Allocator.Temp);
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (section, collider, entity) in SystemAPI.Query<RefRO<PhysicsPositionHash>,RefRO<PhysicsCollider>>().WithEntityAccess())
            {
                //SystemAPI.
                var hashValue = section.ValueRO.HashValue;
                hashmap.Add(hashValue, entity);
                ecb.RemoveComponent<PhysicsPositionHash>(entity);
            }

            //var containsHash = hashmap.ContainsKey(hash);
            var keyList = new NativeList<int>(Allocator.TempJob);
            foreach (var key in hashmap.GetKeyArray(Allocator.Temp))
            {
                if(keyList.Contains(key)) 
                    continue;
                keyList.Add(key);
            }

            //Debug.Log(keyList.Length);
            foreach (var key in keyList)
            {
                var  e           = state.EntityManager.CreateEntity();
                //state.EntityManager.SetName(e, "PhysicsSection");
                var  aabb        = new MinMaxAABB();
                bool defaultAabb = true;
                state.EntityManager.AddBuffer<PhysicsSectionEntities>(e);
                var entities = new NativeList<Entity>(Allocator.Temp);
                
                NativeParallelMultiHashMap<int, Entity>.Enumerator enumerator = hashmap.GetValuesForKey(key);
                
                while (enumerator.MoveNext())
                {
                    var entity = enumerator.Current;
                    
                    // Only add if its an actual physics collider entity
                    if (SystemAPI.HasComponent<PhysicsCollider>(entity))
                    {
                        entities.Add(entity);
                        ecb.AddComponent(entity, new PhysicsSectionTag
                        {
                            Value = e
                        });
                        var localToWorld = state.EntityManager.GetComponentData<LocalToWorld>(entity);
                        var collider     = state.EntityManager.GetComponentData<PhysicsCollider>(entity);
                        var colliderAABB = collider.Value.Value.CalculateAabb(new RigidTransform { pos = localToWorld.Position, rot = localToWorld.Rotation});
                        state.EntityManager.AddComponentData(entity, new PhysicsSectionBoundingVolume
                        {
                            //HashValue = key,
                            BoundingVolume      = colliderAABB.ToMinMaxAABB()
                        });
                        if (defaultAabb)
                        {
                            aabb = colliderAABB.ToMinMaxAABB();
                            defaultAabb = false;
                        }
                        else
                            aabb.Encapsulate(colliderAABB.ToMinMaxAABB());
                    }
                }
                state.EntityManager.AddComponentData(e, new PhysicsSectionBoundingVolume
                {
                    HashValue = key,
                    BoundingVolume = aabb
                });
                var buffer = state.EntityManager.GetBuffer<PhysicsSectionEntities>(e);
                buffer.AddRange(entities.AsArray().Reinterpret<PhysicsSectionEntities>());
            }
            
            ecb.Playback(state.EntityManager);
            hashmap.Dispose();
            keyList.Dispose();
            
        }
    }

    public static class PhysicsGeometryExtensions
    {
        public static MinMaxAABB ToMinMaxAABB(this Aabb aabb)
        {
            var minMaxAABB = new MinMaxAABB
            {
                Min = aabb.Min,
                Max = aabb.Max
            };
            return minMaxAABB;
        }
        
    }
}