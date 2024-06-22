using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    public partial struct VFXSystem : ISystem
    {
        private int _spawnBatchId;
        private int _requestsCountId;
        private int _requestsBufferId;
        private int _datasBufferId;

        private VFXManager<VFXSparksRequest>        sparksManager;
        private VFXManager<VFXSmokeRequest>         smokeManager;
        private VFXManager<VFXBloodHeadshotRequest> bloodHeadshotManager;
        private VFXManager<VFXBloodSprayRequest>    bloodSprayManager;

        private VFXManagerParented<VFXTrailData> trailManager;
        
        
        public const int SparksCapacity  = 1000;
        public const int ExplosionsCapacity = 1000;
        public const int TrailCapacity  = 1000;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ContactWorldSingleton>();
            
            // Names to Ids
            _spawnBatchId     = Shader.PropertyToID("SpawnBatch");
            _requestsCountId  = Shader.PropertyToID("SpawnRequestsCount");
            _requestsBufferId = Shader.PropertyToID("SpawnRequestsBuffer");
            _datasBufferId    = Shader.PropertyToID("DatasBuffer");
            
            // VFX managers
            sparksManager        = new VFXManager<VFXSparksRequest>(SparksCapacity, ref VFXReferences.SparksRequestsBuffer);
            smokeManager         = new VFXManager<VFXSmokeRequest>(SparksCapacity, ref VFXReferences.SmokeRequestsBuffer);
            bloodHeadshotManager = new VFXManager<VFXBloodHeadshotRequest>(SparksCapacity, ref VFXReferences.BloodHeadshotRequestsBuffer);
            bloodSprayManager    = new VFXManager<VFXBloodSprayRequest>(SparksCapacity, ref VFXReferences.BloodSprayRequestsBuffer);
            trailManager         = new VFXManagerParented<VFXTrailData>(TrailCapacity, ref VFXReferences.TrailRequestsBuffer, ref VFXReferences.TrailDatasBuffer);

            // Singletons
            state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXSparksSingleton
            {
                Manager = sparksManager
            });
            state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXSmokeSingleton
            {
                Manager = smokeManager
            });
            state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXBloodHeadshotSingleton
            {
                Manager = bloodHeadshotManager
            });
            state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXBloodSpraySingleton
            {
                Manager = bloodSprayManager
            });
            state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXTrailSingleton
            {
                Manager = trailManager
            });
            /*state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXExplosionsSingleton
            {
                Manager = _explosionsManager,
            });
            state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new VFXThrustersSingleton
            {
                Manager = _thrustersManager,
            });*/
        }
        
        public void OnDestroy(ref SystemState state)
        {
            sparksManager.Dispose(ref VFXReferences.SparksRequestsBuffer);
            smokeManager.Dispose(ref VFXReferences.SmokeRequestsBuffer);
            bloodHeadshotManager.Dispose(ref VFXReferences.BloodHeadshotRequestsBuffer);
            bloodSprayManager.Dispose(ref VFXReferences.BloodSprayRequestsBuffer);
            trailManager.Dispose(ref VFXReferences.TrailRequestsBuffer, ref VFXReferences.TrailDatasBuffer);
        }

        public void OnUpdate(ref SystemState state)
        {
            /*
              // debug force spawn
             bloodHeadshotManager.AddRequest(new VFXBloodHeadshotRequest()
            {
                Position = new Vector3(0f, 15f, 0f)
            });*/
            
            // This is required because we must use data in native collections on the main thread, to send it to VFXGraphs
            SystemAPI.QueryBuilder().WithAll<VFXSparksSingleton>().Build().CompleteDependency();
            SystemAPI.QueryBuilder().WithAll<VFXSmokeSingleton>().Build().CompleteDependency();
            SystemAPI.QueryBuilder().WithAll<VFXBloodHeadshotSingleton>().Build().CompleteDependency();
            SystemAPI.QueryBuilder().WithAll<VFXBloodSpraySingleton>().Build().CompleteDependency();
            SystemAPI.QueryBuilder().WithAll<VFXTrailSingleton>().Build().CompleteDependency();
            
            // Update managers
            float rateRatio = SystemAPI.Time.DeltaTime / Time.deltaTime;
             
            sparksManager.Update(
                VFXReferences.SparksGraph, 
                ref VFXReferences.SparksRequestsBuffer, 
                rateRatio,
                _spawnBatchId,
                _requestsCountId,
                _requestsBufferId);
            
            smokeManager.Update(
                VFXReferences.SmokeGraph, 
                ref VFXReferences.SmokeRequestsBuffer, 
                rateRatio,
                _spawnBatchId,
                _requestsCountId,
                _requestsBufferId);
            
            bloodHeadshotManager.Update(
                VFXReferences.BloodHeadshotGraph, 
                ref VFXReferences.BloodHeadshotRequestsBuffer, 
                rateRatio,
                _spawnBatchId,
                _requestsCountId,
                _requestsBufferId);
            
            bloodSprayManager.Update(
                VFXReferences.BloodSprayGraph, 
                ref VFXReferences.BloodSprayRequestsBuffer, 
                rateRatio,
                _spawnBatchId,
                _requestsCountId,
                _requestsBufferId);
            
            trailManager.Update(
                VFXReferences.TrailGraph, 
                ref VFXReferences.TrailRequestsBuffer, 
                ref VFXReferences.TrailDatasBuffer,
                rateRatio,
                _spawnBatchId,
                _requestsCountId,
                _requestsBufferId,
                _datasBufferId);
        }
    }
}