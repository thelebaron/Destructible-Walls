using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Entities;
using Unity.Transforms;

namespace Junk.Entities
{
    

    /// <summary>
    /// Some extension methods to redirect the flow of reading:
    /// 
    /// ie instead of querying: If my component data "HasComponent" entity {componentDataFromEntity.HasComponent(entity)}
    /// we write it like this: If my entity "HasComponent" component data {entity.HasComponent(ComponentDataFromEntity)}
    /// 
    ///  Make the code flow format make a bit more sense
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this Entity entity, SystemBase system) where T : unmanaged, IComponentData
        {
            return system.EntityManager.HasComponent<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasBuffer<T>(this Entity entity, SystemBase system) where T : unmanaged, IBufferElementData
        {
            return system.EntityManager.HasComponent<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData 
        {
            return entityManager.HasComponent<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static bool HasComponentObject<T>(this Entity entity, EntityManager entityManager) where T : class, ICleanupComponentData
        {
            return entityManager.HasComponent<T>(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static bool HasComponent<T>(this Entity entity, ComponentLookup<T> componentDataFromEntity) where T : unmanaged, IComponentData
        {
            return componentDataFromEntity.HasComponent(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<T>(this Entity entity, BufferLookup<T> bufferFromEntity) where T : unmanaged, IBufferElementData
        {
            return bufferFromEntity.HasBuffer(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, T component, ComponentLookup<T> componentDataFromEntity) where T : unmanaged, IComponentData
        {
            componentDataFromEntity[entity] = component;

            Assert.IsTrue(componentDataFromEntity[entity].Equals(component));
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, T component, SystemBase system) where T : unmanaged, IComponentData
        {
            system.EntityManager.SetComponentData(entity, component);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(this Entity entity, SystemBase system) where T : unmanaged, IComponentData
        {
            return system.EntityManager.GetComponentData<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(this Entity entity, ComponentLookup<T> componentDataFromEntity) where T : unmanaged, IComponentData
        {
            return componentDataFromEntity[entity];
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetComponent<T>(this Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            return entityManager.GetComponentData<T>(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponent<T>(this Entity entity, T component, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            entityManager.AddComponentData(entity, component);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponent<T>(this Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            entityManager.AddComponent<T>(entity);
        }
        
        // Enable/Disable entity
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enable(this Entity entity, EntityManager entityManager)
        {
            if(entityManager.HasComponent<Disabled>(entity))
                entityManager.RemoveComponent<Disabled>(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Disable(this Entity entity, EntityManager entityManager)
        {
            if(!entityManager.HasComponent<Disabled>(entity))
                entityManager.AddComponent<Disabled>(entity);
        }
    }
}