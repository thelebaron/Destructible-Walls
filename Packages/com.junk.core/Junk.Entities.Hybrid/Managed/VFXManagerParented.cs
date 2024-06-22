using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Junk.Entities.Hybrid
{
    public interface IKillableVFX
    {
        public void Kill();
    }

    public struct VFXManagerParented<T> where T : unmanaged, IKillableVFX
    {
        public  NativeReference<int>               RequestsCount;
        public  NativeArray<VFXSpawnToDataRequest> Requests;
        public  NativeArray<T>                     Datas;
        private NativeQueue<int>                   FreeIndexes;
        
        public bool GraphIsInitialized { get; private set; }

        public VFXManagerParented(int maxCount, ref GraphicsBuffer requestsGraphicsBuffer, ref GraphicsBuffer datasGraphicsBuffer)
        {
            RequestsCount = new NativeReference<int>(0, Allocator.Persistent);
            Requests = new NativeArray<VFXSpawnToDataRequest>(maxCount, Allocator.Persistent);
            Datas = new NativeArray<T>(maxCount, Allocator.Persistent);
            FreeIndexes = new NativeQueue<int>(Allocator.Persistent);

            requestsGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxCount,
                Marshal.SizeOf(typeof(VFXSpawnToDataRequest)));
            datasGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxCount,
                Marshal.SizeOf(typeof(T)));

            for (int i = 0; i < maxCount; i++)
            {
                FreeIndexes.Enqueue(i);
            }
            
            GraphIsInitialized = false;
        }

        public void Dispose(ref GraphicsBuffer requestsGraphicsBuffer, ref GraphicsBuffer datasGraphicsBuffer)
        {
            requestsGraphicsBuffer?.Dispose();
            datasGraphicsBuffer?.Dispose();
            if (RequestsCount.IsCreated)
            {
                RequestsCount.Dispose();
            }
            if (Requests.IsCreated)
            {
                Requests.Dispose();
            }
            if (Datas.IsCreated)
            {
                Datas.Dispose();
            }
            if (FreeIndexes.IsCreated)
            {
                FreeIndexes.Dispose();
            }
        }

        public void Update(
            VisualEffect vfxGraph, 
            ref GraphicsBuffer requestsGraphicsBuffer, 
            ref GraphicsBuffer datasGraphicsBuffer, 
            float deltaTimeMultiplier, 
            int spawnBatchId, 
            int requestsCountId, 
            int requestsBufferId, 
            int datasBufferId)
        {
            if (vfxGraph != null && requestsGraphicsBuffer != null && datasGraphicsBuffer != null)
            {
                vfxGraph.playRate = deltaTimeMultiplier;
                
                if (!GraphIsInitialized)
                {
                    vfxGraph.SetGraphicsBuffer(requestsBufferId, requestsGraphicsBuffer);
                    vfxGraph.SetGraphicsBuffer(datasBufferId, datasGraphicsBuffer);
                    GraphIsInitialized = true;
                }

                if (requestsGraphicsBuffer.IsValid() && datasGraphicsBuffer.IsValid())
                {
                    //Debug.Log($"Request count { RequestsCount.Value } ");
                    
                    requestsGraphicsBuffer.SetData(Requests, 0, 0, RequestsCount.Value);
                    datasGraphicsBuffer.SetData(Datas);
                    
                    vfxGraph.SetInt(requestsCountId, math.min(RequestsCount.Value, Requests.Length));
                    vfxGraph.SendEvent(spawnBatchId);
                    
                    RequestsCount.Value = 0;
                }
            }
        }
        
        public int Create()
        {
            if (FreeIndexes.TryDequeue(out int index))
            {
                // Request to spawn
                if (RequestsCount.Value < Requests.Length)
                {
                    Requests[RequestsCount.Value] = new VFXSpawnToDataRequest
                    {
                        IndexInData = index,
                    };
                    RequestsCount.Value++;
                }
                
                
                return index;
            }

            return -1;
        }

        public void Kill(int index)
        {
            if (index >= 0 && index < Datas.Length)
            {
                T killdata = default;
                killdata.Kill();
                Datas[index] = killdata;

                FreeIndexes.Enqueue(index);
            }
        }
    }

}