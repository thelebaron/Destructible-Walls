using System;
using Unity.Assertions;
using Unity.Entities;


namespace thelebaron.bee
{



    public static class EntityExtensions
    {
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static bool HasComponent<T>(this Entity entity, SystemBase system) where T : struct, IComponentData
        {
            return system.EntityManager.HasComponent<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static bool HasComponent<T>(this Entity entity, EntityManager entityManager) where T : struct, IComponentData
        {
            return entityManager.HasComponent<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static bool HasComponent<T>(this Entity entity, ComponentDataFromEntity<T> componentDataFromEntity) where T : struct, IComponentData
        {
            return componentDataFromEntity.HasComponent(entity);
        }

        /// <summary> Make the code flow format make a bit more sense </summary>
        public static void SetComponent<T>(this Entity entity, T component, ComponentDataFromEntity<T> componentDataFromEntity) where T : struct, IComponentData
        {
            componentDataFromEntity[entity] = component;

            Assert.IsTrue(componentDataFromEntity[entity].Equals(component));
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static void SetComponent<T>(this Entity entity, T component, SystemBase system) where T : struct, IComponentData
        {
            system.EntityManager.SetComponentData(entity, component);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static T GetComponent<T>(this Entity entity, SystemBase system) where T : struct, IComponentData
        {
            return system.EntityManager.GetComponentData<T>(entity);
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static T GetComponent<T>(this Entity entity, ComponentDataFromEntity<T> componentDataFromEntity) where T : struct, IComponentData
        {
            return componentDataFromEntity[entity];
        }
        
        /// <summary> Make the code flow format make a bit more sense </summary>
        public static T GetComponent<T>(this Entity entity, EntityManager entityManager) where T : struct, IComponentData
        {
            return entityManager.GetComponentData<T>(entity);
        }
        
        public static void AddComponent<T>(this Entity entity, T component, EntityManager entityManager) where T : struct, IComponentData
        {
            entityManager.AddComponentData(entity, component);
        }
    }
}