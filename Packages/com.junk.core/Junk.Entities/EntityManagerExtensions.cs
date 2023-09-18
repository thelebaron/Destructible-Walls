using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Transforms
{
    
    /// <summary>
    /// Reflection helpers
    /// 
    /// </summary>
    public static class EntityManagerExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field        = obj.GetType().GetField(name, bindingFlags);
            return (T) field?.GetValue(obj);
        }

        /// <summary>
        /// Using reflection, get component data using specific type
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetBoxedComponentData(this EntityManager entityManager, Entity entity, Type type)
        {
            var methodInfo             = typeof(EntityManager).GetMethod("GetComponentData");
            var genericMethodInfo      = methodInfo?.MakeGenericMethod(type);
            var parameters             = new object[] {entity};
            var reflectedComponentData = genericMethodInfo?.Invoke(entityManager, parameters);

            return reflectedComponentData;
        }
        
        public static unsafe IntPtr GetRawComponentDataPtr(this EntityManager entityManager, Entity entity, int typeIndex)
        {
            var methodInfo        = typeof(EntityManager).GetMethod("GetComponentDataRawRO");
            var genericMethodInfo = methodInfo?.MakeGenericMethod();
            var parameters        = new object[] {entity, typeIndex};
            var managedData               = genericMethodInfo?.Invoke(entityManager, parameters);

            return managedData.ToIntPtr();
        }

        public static unsafe void* GetRawComponentData(this EntityManager entityManager, Entity entity, int typeIndex)
        {
            var methodInfo        = typeof(EntityManager).GetMethod("GetComponentDataRawRO");
            var genericMethodInfo = methodInfo?.MakeGenericMethod();
            var parameters        = new object[] {entity, typeIndex};
            var managedData       = genericMethodInfo?.Invoke(entityManager, parameters);

            return (void*)managedData.ToIntPtr();
        }
        
        /// <summary>
        /// Using reflection, set component data per specific type
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="entity"></param>
        /// <param name="componentType"></param>
        /// <param name="componentObject"></param>
        public static void SetComponentObject(this EntityManager entityManager, Entity entity, ComponentType componentType, object componentObject)
        {
            //during retrieval
            Type elementType = componentType.GetManagedType();
            var  listType    = typeof(IComponentData).MakeGenericType(new Type[] {elementType});
            
            Debug.LogError("api needs updating");
            entityManager.SetComponentObject(entity,  componentObject.GetType(),(IComponentData) componentObject); // blegh

            var parameters = new object[] {entity, componentType, componentObject};
            //var methodInfo        = typeof(EntityManager).GetMethod("SetComponentObject");
            // https://stackoverflow.com/questions/8695455/invoke-a-non-generic-method-with-generic-arguments-defined-in-a-generic-class
            /*Type       unboundGenericType = typeof(EntityManager);
            Type       boundGenericType   = unboundGenericType.MakeGenericType(typeof(EntityManager));
            MethodInfo doSomethingMethod  = boundGenericType.GetMethod("SetComponentObject");
            object     instance           = Activator.CreateInstance(boundGenericType);
            doSomethingMethod.Invoke(instance,parameters);
            
            
            var methods           = typeof(EntityManager).GetMethods(BindingFlags.NonPublic|BindingFlags.Instance);
            Debug.Log(methods.Length);*/

            MethodInfo methodInfo = typeof(EntityManager).GetMethod("SetComponentObject", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Debug.Log(methodInfo.Name);
            object data = new float3();
            Debug.LogError("api needs updating");
            //var    tr   = (Translation) data;

            methodInfo?.Invoke(entityManager, parameters);
        }
    }
    
    
    public static class ObjectHandleExtensions
    {
        public static IntPtr ToIntPtr(this object target)
        {
            return GCHandle.Alloc(target).ToIntPtr();
        }

        public static GCHandle ToGcHandle(this object target)
        {
            return GCHandle.Alloc(target);
        }

        public static IntPtr ToIntPtr(this GCHandle target)
        {
            return GCHandle.ToIntPtr(target);
        }
    }
}