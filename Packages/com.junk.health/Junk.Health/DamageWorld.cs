using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Junk.Hitpoints
{
    public struct DamageWorld
    {
        [NoAlias] private NativeList<DamageData>             damageData;
        
        internal NativeStream                                 DamageEventDataStream;
        internal NativeParallelMultiHashMap<Entity, DamageData> EntityDamageIndexMap;
        public   DamageEvents                                 DamageEvents => new DamageEvents(DamageEventDataStream);
        
        public DamageWorld(int numStaticBodies, EntityManager entityManager)
        {
            damageData            = new NativeList<DamageData>(Allocator.Persistent);
            DamageEventDataStream = new NativeStream(1, Allocator.Persistent);
            EntityDamageIndexMap  = new NativeParallelMultiHashMap<Entity, DamageData>(numStaticBodies, Allocator.Persistent);
        }
        
        public void AddDamage(DamageData damage)
        {
            //damageData.Add(damage);
            //var writer = DamageEventDataStream.AsWriter();
            //writer.Write(damage);
            //damageStream.Write(damage);
            EntityDamageIndexMap.Add(damage.Receiver, damage);
        }
        
        public NativeStream.Writer GetDamageWriter()
        {
            return DamageEventDataStream.AsWriter();
        }
        
        public NativeStream.Reader GetDamageReader()
        {
            return DamageEventDataStream.AsReader();
        }
        
        public void Dispose()
        {
            if (damageData.IsCreated)
            {
                damageData.Dispose();
            }
            if (DamageEventDataStream.IsCreated)
            {
                DamageEventDataStream.Dispose();
            }
        }
    }
    
    [BurstCompile]
    internal struct IndexDamageJob : IJobParallelFor
    {
        [ReadOnly] [NativeDisableContainerSafetyRestriction] public NativeStream.Reader                                            StreamReader;
        [WriteOnly] public NativeParallelMultiHashMap<Entity, DamageData>.ParallelWriter EntityDamageIndexMap;
            
        public void Execute(int index)
        {
            StreamReader.Count();
            StreamReader.BeginForEachIndex(index);
            for (var i = 0; i < StreamReader.ForEachCount; i++)
            {
                //var peekedValue = StreamReader.Peek<DamageEvent>();
                var value = StreamReader.Read<DamageData>();
                EntityDamageIndexMap.Add(value.Receiver, value);
            }
            StreamReader.EndForEachIndex();
        }
    }

    [BurstCompile]
    internal partial struct EntityDamageJob : IJobEntity
    {
        [ReadOnly] public NativeParallelMultiHashMap<Entity, DamageData> EntityDamageIndexMap;
        public void Execute(Entity entity, DynamicBuffer<HealthDamageBuffer> buffer)
        {
            var containsEntity = EntityDamageIndexMap.ContainsKey(entity);
            if(!containsEntity)
            {
                return;
            }
            //NativeParallelMultiHashMap<Entity, DamageEvent>.Enumerator damageEvents   = EntityDamageIndexMap.GetValuesForKey(entity);
            //EntityDamageIndexMap.TryGetFirstValue(entity, out var damageEvent, out var hashMapIterator);

            //Debug.Log($"Entity: {entity}");
            
            foreach (var damageEvent in EntityDamageIndexMap.GetValuesForKey(entity))
            {
                buffer.Add(new HealthDamageBuffer { Value = damageEvent });
            }
        }
    }
    
    [BurstCompile]
    internal  struct ClearDamageJob : IJob
    {
        public DamageWorld DamageWorld;
        public void Execute()
        {
            //DamageWorld.DamageEventDataStream.Dispose();
            //DamageWorld.DamageEventDataStream = new NativeStream(1, Allocator.Persistent);
            
            DamageWorld.EntityDamageIndexMap.Clear();
        }
    }
}