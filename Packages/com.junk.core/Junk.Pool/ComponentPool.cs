using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Junk.Pool
{
    public interface IPoolable<T> where T : IPoolable<T>
    {
        public ComponentPool<T> Pool { get; set; }
    }
    
    public class ComponentPool<T> where T : IPoolable<T>
    {
        readonly Stack<T>       m_Stack = new Stack<T>();
        readonly Func<T>        m_ActionOnCreate;
        readonly UnityAction<T> m_ActionOnGet;
        readonly UnityAction<T> m_ActionOnRelease;
        readonly bool           m_CollectionCheck = true;

        /// <summary>
        /// Number of objects in the pool.
        /// </summary>
        public int countAll { get; private set; }

        /// <summary>
        /// Number of active objects in the pool.
        /// </summary>
        public int countActive
        {
            get { return countAll - countInactive; }
        }

        /// <summary>
        /// Number of inactive objects in the pool.
        /// </summary>
        public int countInactive
        {
            get { return m_Stack.Count; }
        }

        public ComponentPool(Func<T> createPooledItem, UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease, bool collectionCheck = true)
        {
            m_ActionOnCreate  = createPooledItem;
            m_ActionOnGet     = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_CollectionCheck = collectionCheck;
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        private T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = m_ActionOnCreate();
                element.Pool = this;
                //element = new T();
                countAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }

            if (m_ActionOnGet != null)
                m_ActionOnGet(element);
            return element;
        }

        /// <summary>
        /// Pooled object.
        /// </summary>
        public struct PoolElement : IDisposable
        {
            readonly T             m_ToReturn;
            readonly ComponentPool<T> m_Pool;

            internal PoolElement(T value, ComponentPool<T> pool)
            {
                m_ToReturn = value;
                m_Pool     = pool;
            }

            /// <summary>
            /// Disposable pattern implementation.
            /// </summary>
            void IDisposable.Dispose() => m_Pool.Release(m_ToReturn);
        }

        /// <summary>
        /// Get a new PooledObject.
        /// </summary>
        /// <param name="element">Output new typed object.</param>
        /// <returns>New PooledObject</returns>
        //public PoolElement ZGet(out T element) => new PoolElement(element = Get(), this);
        public PoolElement GetElement(out T element)
        {
            var e = new PoolElement(element = Get(), this);

            element.Pool = this;
            
            return e;
        }

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="element">Object to release.</param>
        public void Release(T element)
        {
#if UNITY_EDITOR // keep heavy checks in editor
            if (m_CollectionCheck && m_Stack.Count > 0)
            {
                if (m_Stack.Contains(element))
                    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }
#endif
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);
        }

        public void Resize(int i)
        {
            if (i < 0)
                return;
            while (i > m_Stack.Count)
            {
                var element = m_ActionOnCreate();
                element.Pool = this;
                m_Stack.Push(element);
            }
        }
    }
}