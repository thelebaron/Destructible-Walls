using Unity.Entities;

namespace Junk.Entities.Hybrid
{
    public struct VFXSparksSingleton : IComponentData
    {
        public VFXManager<VFXSparksRequest> Manager;
    }
    
    public struct VFXSmokeSingleton : IComponentData
    {
        public VFXManager<VFXSmokeRequest> Manager;
    }
    
    public struct VFXBloodHeadshotSingleton : IComponentData
    {
        public VFXManager<VFXBloodHeadshotRequest> Manager;
    }
    
    public struct VFXBloodSpraySingleton : IComponentData
    {
        public VFXManager<VFXBloodSprayRequest> Manager;
    }
    
    public struct VFXTrailSingleton : IComponentData
    {
        public VFXManagerParented<VFXTrailData> Manager;
    }
}