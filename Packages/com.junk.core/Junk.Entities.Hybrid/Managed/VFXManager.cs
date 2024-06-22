using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Junk.Entities.Hybrid
{
    public struct VFXManager<T> where T : unmanaged
    {
        public NativeReference<int> RequestsCount;
        public NativeArray<T>       Requests;

        public bool GraphIsInitialized { get; private set; }

        // ReSharper disable once RedundantAssignment
        public VFXManager(int maxRequests, ref GraphicsBuffer graphicsBuffer)
        {
            RequestsCount = new NativeReference<int>(0, Allocator.Persistent);
            Requests      = new NativeArray<T>(maxRequests, Allocator.Persistent);

            graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxRequests, Marshal.SizeOf(typeof(T)));
            
            GraphIsInitialized = false;
        }

        public void Dispose(ref GraphicsBuffer graphicsBuffer)
        {
            graphicsBuffer?.Dispose();
            if (RequestsCount.IsCreated)
            {
                RequestsCount.Dispose();
            }
            if (Requests.IsCreated)
            {
                Requests.Dispose();
            }
        }

        public void Update(
            VisualEffect       vfxGraph, 
            ref GraphicsBuffer graphicsBuffer, 
            float              deltaTimeMultiplier, 
            int                spawnBatchId, 
            int                requestsCountId, 
            int                requestsBufferId)
        {
            if (vfxGraph != null && graphicsBuffer != null)
            {
                vfxGraph.playRate = deltaTimeMultiplier;
            
                if (!GraphIsInitialized)
                {
                    vfxGraph.SetGraphicsBuffer(requestsBufferId, graphicsBuffer);
                    GraphIsInitialized = true;
                }

                if (graphicsBuffer.IsValid())
                {
                    //Debug.Log("Sending batch of " + RequestsCount.Value + " requests");
                    
                    graphicsBuffer.SetData(Requests, 0, 0, RequestsCount.Value);
                    
                    vfxGraph.SetInt(requestsCountId, math.min(RequestsCount.Value, Requests.Length));
                    vfxGraph.SendEvent(spawnBatchId);
                    RequestsCount.Value = 0;
                }
            }
        }

        public void AddRequest(T request)
        {
            if (RequestsCount.Value < Requests.Length)
            {
                Requests[RequestsCount.Value] = request;
                RequestsCount.Value++;
            }
        }
    }
}