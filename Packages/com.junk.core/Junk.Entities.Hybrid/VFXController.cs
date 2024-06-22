using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Junk.Entities.Hybrid
{
    /// <summary>
    /// Controls minor behaviour to its attached VisualEffect
    /// https://docs.unity3d.com/ScriptReference/VFX.VisualEffect.SendEvent.html
    /// </summary>
    [ExecuteAlways]
    public class VFXController : MonoBehaviour
    {
        public                  VisualEffect visualEffect;
        private static readonly int          s_InitialPositionId = Shader.PropertyToID("Initial Position");
        private static readonly int          s_InitialAngleId    = Shader.PropertyToID("Initial Euler Angle");
        private static readonly int          s_InitialVelocityId = Shader.PropertyToID("Initial Velocity");
        private static readonly int          s_DirectionId       = Shader.PropertyToID("Direction");
        private static readonly int          s_NormalId          = Shader.PropertyToID("Normal");
        private static readonly int          s_CountId           = Shader.PropertyToID("Count");
        private static readonly int          s_OldSizeId            = Shader.PropertyToID("Size");
        private static readonly int          s_SpeedId           = Shader.PropertyToID("Speed");
        private static readonly int          s_GravityId         = Shader.PropertyToID("Gravity");
        private static readonly int          s_LifeId            = Shader.PropertyToID("LifeMinMax");
        private static readonly int          s_BurstId           = Shader.PropertyToID("Burst");

        private static readonly int               s_EventPlayId  = Shader.PropertyToID("OnPlay");
        private static readonly int               s_ColorID      = Shader.PropertyToID("color");
        private static readonly int               s_PositionID   = Shader.PropertyToID("position");        
        private static readonly int               s_SizeID       = Shader.PropertyToID("size");

        private static readonly int               s_SpawnCountID = Shader.PropertyToID("spawnCount");
        private                 VFXEventAttribute eventAttribute;
        
        private                  GraphicsBuffer       graphicsBuffer;
        private const            int                  bufferStride          = 12; // 12 Bytes for a Vector3 (4,4,4)
        private static readonly  int                  VfxBufferProperty     = Shader.PropertyToID("MyBuffer");
        [SerializeField] private int                  bufferInitialCapacity = 1024;
        public                   List<Vector3>        myData                = new List<Vector3>();
        private                  NativeArray<Vector3> m_BufferData;
        private                  float                timer;
        public                   bool                 testing;
        
        public void Awake()
        {
            visualEffect   = GetComponent<VisualEffect>();
            eventAttribute = visualEffect.CreateVFXEventAttribute();
            //EnsureBufferCapacity(ref graphicsBuffer, bufferInitialCapacity, bufferStride, visualEffect, VfxBufferProperty);
            //m_BufferData = new NativeArray<Vector3>(128, Allocator.Persistent);
        }

        public void Update()
        {
            if (testing)
            {
                if(eventAttribute==null)
                    eventAttribute =  visualEffect.CreateVFXEventAttribute();
                timer          += Time.deltaTime;
            
                if(timer > 1.1f)
                {
                    timer = 0;
                    for (int i = 0; i < 1; i++)
                    {
                        SendPositionCountEvent(new Vector3(0,i,0), 606);
                    }
                }
            }

        }

        void OnDestroy()
        {
            //ReleaseBuffer(ref graphicsBuffer);
            //m_BufferData.Dispose();
        }
        
        private void EnsureBufferCapacity(ref GraphicsBuffer buffer, int capacity, int stride, VisualEffect vfx, int vfxProperty)
        {
            // Reallocate new buffer only when null or capacity is not sufficient
            if (buffer == null || buffer.count < capacity)
            {
                // Buffer memory must be released
                buffer?.Release();
                // Vfx Graph uses structured buffer
                buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, capacity, stride);
                // Update buffer referenece
                vfx.SetGraphicsBuffer(vfxProperty, buffer);
            }
        }
 
        private void ReleaseBuffer(ref GraphicsBuffer buffer)
        {
            // Buffer memory must be released
            buffer?.Release();
            buffer = null;
        }

        
        public void SetBurst(bool burst)
        {
            visualEffect.SetBool(s_BurstId, burst);
        }
        
        public void SetPosition(float3 position)
        {
            visualEffect.SetVector3(s_InitialPositionId, position);
        }
        
        public void SetDirection(float3 value)
        {
            visualEffect.SetVector3(s_DirectionId, value);
        }
        
        public void Play(float3 position)
        {
            visualEffect.SetVector3(s_InitialPositionId, position);
            visualEffect.Play();
            //visualEffect.SendEvent(EventPlayId, eventAttribute);
        }
        

        
        public void SetCount(int count)
        {
            visualEffect.SetInt(s_CountId, count);
        }
    
        public void SetNormal(Vector3 normal)
        {
            if(!visualEffect.HasVector3(s_NormalId))
                Debug.Log(gameObject.name);
            visualEffect.SetVector3(s_NormalId, normal);
        }
        
        // controlled by new vfxbridge system
        // Sets the initial euler angle of the particle
        public void SetAngle(Vector3 angle)
        {
            visualEffect.SetVector3(s_InitialAngleId, angle);
        }
        
        public void SetVelocity(float3 value)
        {
            visualEffect.SetVector3(s_InitialVelocityId, value);
        }
        
        public void SetSize(float size)
        {
            visualEffect.SetFloat(s_OldSizeId, size);
        }
        
        public void SetSpeed(float speed)
        {
            if(!visualEffect.HasFloat(s_SpeedId))
                Debug.Log(gameObject.name);
            visualEffect.SetFloat(s_SpeedId, speed);
        }
        
        public void SetGravity(Vector3 grav)
        {
            visualEffect.SetVector3(s_GravityId, grav);
        }
        
        /// <summary>
        /// Sets the Lifetime of a particle
        /// </summary>
        /// <param name="life">Min & Max values as float2</param>
        public void SetLife(float2 life)
        {
            visualEffect.SetVector2(s_LifeId, new Vector2(life.x/6, life.y));
        }

        public void Stop()
        {
            visualEffect.Stop();
            gameObject.SetActive(false);
        }
        
        public void OnDisable()
        {
            visualEffect.Stop();
        }
        
        #region EventAttributes
        
        public void EventSpawnCount(int count) => eventAttribute.SetInt(s_SpawnCountID, count);
        public void EventPosition(float3 position) => eventAttribute.SetVector3(s_PositionID, position);
        public void SendEvent() => visualEffect.SendEvent(s_EventPlayId, eventAttribute);

        public void SendEventPosition(float3 position)
        {
            eventAttribute.SetVector3(s_PositionID, position);
            visualEffect.SendEvent(s_EventPlayId, eventAttribute);
        } 
        
        /// <summary>
        /// Send event with position and count
        /// </summary>
        public void SendPositionCountEvent(float3 position, float size = 0.1f, int count = 1)
        {
            //Debug.Log("SendPositionCountEvent event" + size + " " + count);
            eventAttribute.SetVector3(s_PositionID, position);
            eventAttribute.SetFloat(s_SizeID, size);
            eventAttribute.SetFloat(s_SpawnCountID, count);
            visualEffect.SendEvent(s_EventPlayId, eventAttribute);
        }
        
        #endregion
    }
}

