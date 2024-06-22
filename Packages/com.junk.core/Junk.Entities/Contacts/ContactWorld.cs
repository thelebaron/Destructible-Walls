using System;
using Junk.Entities;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Junk.Entities
{
    /// <summary>
    /// Contacts have several distinct sizes:
    /// Tiny - Bullet impacts, blood drips - 0.1m
    /// Small - Large caliber weapons - 0.25m
    /// Medium - Blood splatter - 0.5m
    /// Large - Craters - 1m
    /// </summary>
    public struct ContactWorld : IDisposable
    {
        [NoAlias] private  NativeArray<ContactData> contacts;
        [NoAlias] private  NativeList<ContactData>  pendingContacts;
        [NoAlias] internal NativeParallelHashMap<Entity, int>              EntityBodyIndexMap;
        public             NativeParallelMultiHashMap<int, ContactData>    TinyContactSpatialMap;  // 0.1m
        public             NativeParallelMultiHashMap<int, ContactData>    SmallContactSpatialMap; // 0.1m
        [NoAlias] private  NativeParallelMultiHashMap<Entity, ContactData> contactEntityIndexMap;
        private            PrefabEntityData                                prefabEntityData;
        
        public NativeArray<ContactData> Contacts        => contacts;
        public NativeList<ContactData>  PendingContacts => pendingContacts;
        
        public Entity BulletholeTinyPrefab => prefabEntityData.BulletHoleTiny;
        public Entity BloodSplatTinyPrefab => prefabEntityData.BloodSplatTiny;
        
        public ContactWorld(int numStaticBodies, EntityManager entityManager)
        {
            contacts           = new NativeArray<ContactData>(numStaticBodies, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            pendingContacts    = new NativeList<ContactData>(Allocator.Persistent);
            EntityBodyIndexMap = new NativeParallelHashMap<Entity, int>(contacts.Length, Allocator.Persistent);
            
            //Broadphase = new Broadphase(numStaticBodies, numDynamicBodies);
            TinyContactSpatialMap = new NativeParallelMultiHashMap<int, ContactData>(contacts.Length, Allocator.Persistent);
            SmallContactSpatialMap = new NativeParallelMultiHashMap<int, ContactData>(contacts.Length, Allocator.Persistent);
            
            contactEntityIndexMap = new NativeParallelMultiHashMap<Entity, ContactData>(contacts.Length, Allocator.Persistent);
            
            prefabEntityData = default;
        }
        
        public void UpdatePrefabs(EntityManager entityManager)
        {
            // BovineLabs has a helper method but for now dont want to include another dependency
            using var query = new EntityQueryBuilder(Allocator.Temp).WithAll<PrefabEntityData>()
                .WithOptions(EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeSystems | EntityQueryOptions.Default)
                .Build(entityManager);
            query.CompleteDependency();
            if (query.CalculateEntityCount() < 1)
            {
                Debug.LogError("No PrefabEntityData found, please add a ContactsAuthoring to a subscene(preferrably the main settings subscene).");
                prefabEntityData = default;
                return;
            }
            prefabEntityData = query.GetSingleton<PrefabEntityData>();
        }
        
        /// <summary>
        /// Resize the internal arrays to the specified capacity.
        /// </summary>
        private void SetCapacity(int numBodies)
        {
            // Increase body storage if necessary
            if (contacts.Length < numBodies)
            {
                contacts.Dispose();
                contacts                       = new NativeArray<ContactData>(numBodies, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                EntityBodyIndexMap.Capacity    = contacts.Length;
                TinyContactSpatialMap.Capacity = contacts.Length;
                contactEntityIndexMap.Capacity = contacts.Length;
            }
        }
        
        public ContactData CreateContact(ContactType type, LocalToWorld localToWorld, RaycastHit hit = default, Entity parent = new Entity())
        {
            var contact = new ContactData
            {
                LocalToWorld = localToWorld,
                Parent       = parent,
                Type         = type,
                Hit          = hit
            };
            pendingContacts.Add(contact);
            return contact;
        }
        
        public ContactData CreateContact(ContactType type, float3 position, quaternion rotation, float2 scale, RaycastHit hit = default, Entity parent = new Entity())
        {
            var contact = new ContactData
            {
                ScaleMinMax = scale,
                LocalToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(position, rotation, new float3(scale.x, scale.x, scale.x))
                },
                Parent       = parent,
                Type         = type,
                Hit          = hit
            };
            pendingContacts.Add(contact);
            return contact;
        }
        
        /// <summary>
        /// Constructor for visualeffect particles
        /// </summary>
        public ContactData CreateVFXContact(ContactType type, float3 position, float3 angle, float3 direction, float size = 0.1f, int spawnCount = 1)
        {
            //Assert.IsTrue(type == ContactType.VFXBloodMist);
            //Debug.Log(size);
            var contact = new ContactData
            {
                Type = type,
                Position = position,
                Angle = angle,
                Direction = direction,
                Size = size,
                SpawnCount = spawnCount,
            };
            pendingContacts.Add(contact);
            return contact;
        }
        
        public void AddContact(Entity entity, ContactData contact)
        {
            contactEntityIndexMap.Add(entity, contact);
        }
        
        public void ClearAllContacts()
        {
            TinyContactSpatialMap.Clear();
            SmallContactSpatialMap.Clear();
            contactEntityIndexMap.Clear();
        }
        
        public void Dispose()
        {
            contacts.Dispose();
            pendingContacts.Dispose();
            EntityBodyIndexMap.Dispose();
            TinyContactSpatialMap.Dispose();
            SmallContactSpatialMap.Dispose();
            contactEntityIndexMap.Dispose();
        }
    }
}