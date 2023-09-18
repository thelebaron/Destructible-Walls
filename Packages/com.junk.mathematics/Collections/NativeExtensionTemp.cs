using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Junk.Collections
{
    unsafe public static class NativeExtensionTemp
    {
		/// <summary>
		/// Copypasted from joachim animation repo: Will this be replaced by an official extension?
		/// </summary>
        public static NativeArray<U> Reinterpret_Temp<T, U>(this NativeArray<T> array) where U : unmanaged where T : unmanaged
        {
            var tSize = UnsafeUtility.SizeOf<T>();
            var uSize = UnsafeUtility.SizeOf<U>();

            var byteLen = ((long) array.Length) * tSize;
            var uLen    = byteLen / uSize;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (uLen * uSize != byteLen)
            {
                throw new InvalidOperationException($"Types {typeof(T)} (array length {array.Length}) and {typeof(U)} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.");
            }
#endif
            var ptr    = NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array);
            var result = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<U>(ptr, (int) uLen, Allocator.Invalid);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var handle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref result, handle);
#endif

            return result;
        }
        
        public static unsafe void ClearArray<T>(this NativeArray<T> array/*, int length*/) where T : unmanaged
        {
            UnsafeUtility.MemClear(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), UnsafeUtility.SizeOf<T>() * array.Length /*length*/);
        }
    }
}