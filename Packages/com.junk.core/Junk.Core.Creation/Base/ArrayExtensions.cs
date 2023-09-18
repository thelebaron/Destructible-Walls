using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Core.Creation
{
    unsafe public static class UnsafeArrayExtensions
    {
        /// <summary>
        /// need a test for this
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <returns></returns>
        public static T[] AsType<T,TK>(this ref BlobArray<TK> value) where T : struct where TK : struct
        {
            var result = new T[value.Length];
            if (value.Length > 0)
            {
                var src = value.GetUnsafePtr();

                var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
                var addr   = handle.AddrOfPinnedObject();

                UnsafeUtility.MemCpy((void*)addr, src, value.Length * UnsafeUtility.SizeOf<TK>());

                handle.Free();
            }
            return result;
        }

    }
    
    unsafe public static class ArrayExtensions
    {

        public static Matrix4x4[] ToMatrix4x4(this float4x4[] value)
        {
            var array = new Matrix4x4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
                
        public static Vector4[] ToVector4(this float4[] value)
        {
            var array = new Vector4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
        public static Vector3[] ToVector3(this float3[] value)
        {
            var array = new Vector3[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
        
        public static Vector2[] ToVector2(this float2[] value)
        {
            var array = new Vector2[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
        public static Color[] ToColor(this colour[] value)
        {
            var array = new Color[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
        
        // and reverse
        public static float2[] ToFloat2(this Vector2[] value)
        {
            var array = new float2[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
        public static float3[] ToFloat3(this Vector3[] value)
        {
            var array = new float3[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
                
        public static float4[] ToFloat3(this Vector4[] value)
        {
            var array = new float4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
        
        public static float4x4[] ToFloat3(this Matrix4x4[] value)
        {
            var array = new float4x4[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }     

        public static colour[] ToColor(this Color[] value)
        {
            var array = new colour[value.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value[i];
            }
            
            return array;
        }
    }
}