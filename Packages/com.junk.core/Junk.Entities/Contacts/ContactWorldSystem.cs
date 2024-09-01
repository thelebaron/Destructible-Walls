using System;
using Junk.Math;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Junk.Entities
{
    public struct ContactWorldSingleton : IComponentData
    {
        public ContactWorld ContactWorld;

    }

    public struct ContactData : IComponentData
    {
        public LocalToWorld LocalToWorld;
        public float2       ScaleMinMax;
        public ContactType  Type;
        public RaycastHit   Hit;
        public Entity       Parent;

        // VFX parameters
        public float3 Position;
        public float3 Angle;
        public float3 Direction;
        public float  Size; // the uniform size for a vfx particle, default is 0.1f
        public int    SpawnCount;
        
    }
    
    public enum ContactType
    {
        DecalBulletTiny,
        DecalBulletSmall,
        DecalBloodSmall,
        DecalBloodMedium,
        DecalBloodLarge,
        DecalCrater,
        VFXBloodMist,
        VFXBloodSpray1,
    }
    
    public partial struct ContactWorldSystem : ISystem, ISystemStartStop
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.AddComponentData(state.SystemHandle, new ContactWorldSingleton()
            {
                ContactWorld = new ContactWorld(100, state.EntityManager)
            });
            state.RequireForUpdate<PrefabEntityData>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            if(!SystemAPI.HasSingleton<PrefabEntityData>())
                Debug.Log("Missing singleton prefabentitydata.");
            
            var contactWorldSingleton = state.EntityManager.GetComponentData<ContactWorldSingleton>(state.SystemHandle);
            contactWorldSingleton.ContactWorld.UpdatePrefabs(state.EntityManager);
            state.EntityManager.SetComponentData(state.SystemHandle, contactWorldSingleton);
        }
        
        [BurstCompile]
        public void OnStopRunning(ref SystemState state)
        {
            
        }
        
        /// <summary>
        ///     Adds all npc entities to a HashMap(dictionary) with a positional location.
        ///     Should be fairly well optimized.
        /// </summary>
        [BurstCompile]
        private struct HashPositionJob : IJob
        {
            public EntityCommandBuffer CommandBuffer;
            public ContactWorld        ContactWorld;
            public Random              Random;
            public void Execute()
            {
                const float radiusTiny = 0.05f;
                const float radiusSmall = 0.25f;
                //var incomingContacts =
                var contactsTiny = ContactWorld.TinyContactSpatialMap;
                var contactsSmall = ContactWorld.TinyContactSpatialMap;
                for (var i = 0; i < ContactWorld.PendingContacts.Length; i++)
                {
                    var pendingContact = ContactWorld.PendingContacts[i];

                    switch (pendingContact.Type)
                    {                        
                        case ContactType.DecalBulletTiny:
                            
                            // Notes for future use - This(cellRadius) cannot be a different value per different faction,
                            // as a unique radius(the value used for hashing) filters out differently hashed entities, which for a general purpose targeting
                            // system is probably unwanted.
                            var hash = GetHashedPosition(pendingContact.LocalToWorld.Position, radiusTiny);

                            if (contactsTiny.ContainsKey(hash))
                            {
                                //Debug.Log(pendingContact.LocalToWorld.Position + " is already in the hashmap");
                                continue;
                            }
                            contactsTiny.Add(hash, pendingContact);
                            var contactEntity = CreateBulletholeDecal(hash, pendingContact); // note the entity is a deferred ecb entity and not valid unless remapped
                            break;
                        case ContactType.DecalBulletSmall:

                            break;
                        case ContactType.DecalBloodSmall:
                            // Notes for future use - This(cellRadius) cannot be a different value per different faction,
                            // as a unique radius(the value used for hashing) filters out differently hashed entities, which for a general purpose targeting
                            // system is probably unwanted.
                            hash = GetHashedPosition(pendingContact.LocalToWorld.Position, radiusSmall);

                            if (contactsSmall.ContainsKey(hash))
                            {
                                //Debug.Log(pendingContact.LocalToWorld.Position + " is already in the hashmap");
                                continue;
                            }
                            contactsSmall.Add(hash, pendingContact);
                            
                            RaycastHit hit = pendingContact.Hit;
                            
                            var smallbloodDecalContactEntity = CreateTinyBloodDecal(hash, pendingContact); // note the entity is a deferred ecb entity and not valid unless remapped
                            break;
                        case ContactType.DecalBloodMedium:
                            break;
                        case ContactType.DecalBloodLarge:
                            break;
                        case ContactType.DecalCrater:
                            break;
                        case ContactType.VFXBloodMist:
                            break;
                        case ContactType.VFXBloodSpray1:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                ContactWorld.PendingContacts.Clear();
            }
            
            public Entity CreateBulletholeDecal(int positionHash, ContactData data)
            {
                // Get a random offset to combine with the normal to prevent z fighting
                var decal         = CommandBuffer.Instantiate(ContactWorld.BulletholeTinyPrefab);
                var randomOffset  = Random.NextFloat(0.002f, 0.01f);
                var randomScale   = Random.NextFloat(0.05f, 0.2f);
                var surfaceNormal = data.Hit.SurfaceNormal;
                
                if (surfaceNormal.Equals(maths.up) || surfaceNormal.Equals(-maths.up))
                {
                    var randomTilt = new float3(Random.NextFloat(-0.02f, 0.02f), 0, Random.NextFloat(-0.02f, 0.02f));
                    if (randomTilt.Equals(0f))
                        randomTilt = 0.03f;
                    
                    surfaceNormal += randomTilt;
                }

                var position = data.Hit.Position + surfaceNormal * randomOffset;
                var rot = math.mul(quaternion.LookRotationSafe(-surfaceNormal, maths.up), quaternion.Euler(0, 0, Random.NextFloat(0f, 360f)));

                var localTransform = LocalTransform.FromPositionRotationScale(position, rot, randomScale);
                // NOTE! normal must be negative(or depending on the facing dir of the quad being used)
                // otherwise the mesh shows as black even if both sides are rendered
                //EntityCommandBuffer.SetComponent(decal, new Rotation { Value = /*quaternion.LookRotationSafe(-surfaceNormal, maths.up)*/ });
                // debugging
                CommandBuffer.AddComponent(decal, new ContactDebugData {RaycastHit = data.Hit });
                CommandBuffer.SetComponent(decal, localTransform);
                
                
                return decal;
            }
            
            public Entity CreateTinyBloodDecal(int positionHash, ContactData data)
            {
                if(data.ScaleMinMax.Equals(0f))
                    data.ScaleMinMax = new float2(0.05f, 0.5f);
                // Get a random offset to combine with the normal to prevent z fighting
                var decal         = CommandBuffer.Instantiate(ContactWorld.BloodSplatTinyPrefab);
                var randomOffset  = Random.NextFloat(0.0001f, 0.001f);
                var randomScale   = Random.NextFloat(data.ScaleMinMax.x, data.ScaleMinMax.y);
                var surfaceNormal = data.Hit.SurfaceNormal;
                
                //if (surfaceNormal.Equals(maths.up) || surfaceNormal.Equals(-maths.up))
                {
                    var randomTilt = new float3(Random.NextFloat(-0.0001f, 0.0001f), 0, Random.NextFloat(-0.0001f, 0.0001f));
                    if (randomTilt.Equals(0f))
                        randomTilt = 0.0001f;
                    
                    surfaceNormal += randomTilt;
                }

                var position = data.Hit.Position + ((math.normalizesafe(surfaceNormal)) * randomOffset);
                var rot      = math.mul(quaternion.LookRotationSafe(-surfaceNormal, maths.up), quaternion.Euler(0, 0, Random.NextFloat(0f, 360f)));

                var localTransform = LocalTransform.FromPositionRotationScale(position, rot, randomScale);
                // NOTE! normal must be negative(or depending on the facing dir of the quad being used)
                // otherwise the mesh shows as black even if both sides are rendered
                //EntityCommandBuffer.SetComponent(decal, new Rotation { Value = /*quaternion.LookRotationSafe(-surfaceNormal, maths.up)*/ });
                
                
                CommandBuffer.AddComponent(decal, new ContactDebugData {RaycastHit = data.Hit });
                
                CommandBuffer.SetComponent(decal, localTransform);
                
                //EntityCommandBuffer.AddComponent(index, decal, new MaterialMainTexUv {Value = new float4(1, 1, 0, 0)});
                // quick blood decal implementation, no equivalent decaldata component for this yet(burst error)
                /*CommandBuffer.SetComponent(decal, new BulletDecalData {
                    Value        = 1337,//Count.Value, 
                    PositionHash = positionHash
                });*/
                
                //var count = Count.Value;// does this work?
                //count++;
                //Count.Value = count;
                
                return decal;
            }
        }

        /// <summary>
        ///     Iterate over hashmap and add potential targets that are within the same cell.
        ///     If the buffer has target/s in it, skip that entity. Otherwise attempt to add nearby entities.
        ///     Also dont add npcs that have the same team index todo: check for allied/hostile teams?
        /// </summary>
        [BurstCompile]
        private struct CheckHashedPositionsJob : IJobChunk
        {
            public            float                                        CellRadius;
            [ReadOnly] public EntityTypeHandle                             EntityTypeHandle;
            [ReadOnly] public ComponentLookup<LocalToWorld>                LocalToWorldData;
            [ReadOnly] public NativeParallelMultiHashMap<int, ContactData> HashMap;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in                 v128           chunkEnabledMask)
            {
                var entities       = chunk.GetNativeArray(EntityTypeHandle);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var entity          = entities[i];

                    // Break out of target finding if theres already targets to sort
                    //if (targetEntities.Length > 0)
                        //continue;

                    // Notes for future use - if you get rid of this stored value, need to compute it at runtime and it must match the hash algorithm that was used to fill the hashmap
                    //var hash = personBehaviour.DebugSelfPositionHash;//(int) math.hash(new int3(math.floor(positon / personBehaviour.SightRadius)));
                    var hash         = GetHashedPosition(LocalToWorldData[entity].Position, CellRadius);
                    var containsHash = HashMap.ContainsKey(hash);

                    if (!containsHash)
                        continue;

                    var tryGetFirstValue = HashMap.TryGetFirstValue(hash, out var e, out var hashMapIterator);
                    if (tryGetFirstValue)
                    {
                        //if (e.Equals(entity))
                            //continue;
                        //if (e.Equals(Entity.Null))
                            //continue;
                        //Debug.Log("entity " + entity + " is trying to add " + e);
                        //targetEntities.Add(new TargetElement
                            //{ Value = new TargetInfo { Entity = e, Position = LocalToWorldData[e].Position } });
                    }

                    // Add more targets
                    for (var j = 0; j < 30; j++)
                        if (HashMap.TryGetNextValue(out e, ref hashMapIterator))
                        {
                            //if (e.Equals(entity))
                                //continue;
                            //if (e.Equals(Entity.Null))
                                //continue;
                            //Debug.Log("entity " + entity + " is trying to add " + e);
                            //targetEntities.Add(new TargetElement
                                //{ Value = new TargetInfo { Entity = e, Position = LocalToWorldData[e].Position } });
                        }
                        else
                        {
                            break;
                        }
                }
            }
        }
        
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new HashPositionJob
            {
                CommandBuffer = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                ContactWorld = SystemAPI.GetSingletonRW<ContactWorldSingleton>().ValueRW.ContactWorld,
                Random = Random.CreateFromIndex(state.LastSystemVersion)
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            var contacts = state.EntityManager.GetComponentData<ContactWorldSingleton>(state.SystemHandle);
            contacts.ContactWorld.Dispose();
            state.EntityManager.RemoveComponent<ContactWorldSingleton>(state.SystemHandle);
        }
        
        private static int GetHashedPosition(float3 position, float cellRadius)
        {
            return (int)math.hash(new int3(math.floor(position / cellRadius)));
        }

        public static void ResetAsync(WorldUnmanaged world)
        {
            var       builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Contact>();
            using var q       = world.EntityManager.CreateEntityQuery(builder);
            world.EntityManager.DestroyEntity(q);
        }
    }
}