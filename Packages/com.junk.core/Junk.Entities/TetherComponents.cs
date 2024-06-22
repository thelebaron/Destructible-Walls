using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    public struct TetherData : IComponentData
    {
        public float Rate;
        public float Elapsed;
        public float DistanceToPlayer;
        public bool PlayerVisible;
        public float VisibileScore;
    }

    public struct TetherSamplePoint : IBufferElementData
    {
        public float3 Point;
        public bool   Valid;
        public bool   CanSeePlayer;
    }
    
    
    
    
    
    
    
    
    public struct TetherSingleton : IComponentData
    {
        public float DummyData;
    }
    
    public struct WorldTetherPoint : IBufferElementData
    {
        public Entity Entity;
        public float3 Position;
        public float  Visibility;
        public bool  CanSeePlayer;
    }
}