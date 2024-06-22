using UnityEngine;
using UnityEngine.VFX;

namespace Junk.Entities.Hybrid
{
    // for how many vfx particles we need to spawn for particles like the thruster
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXSpawnToDataRequest
    {
        public int IndexInData;
    }
    
    /*
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXHitSparksRequest
    {
        public Vector3 Position;
        public Vector3 Color;
    }

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXExplosionRequest
    {
        public Vector3 Position;
        public float   Scale;
    }


    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXThrusterData : IKillableVFX
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Color;
        public float   Size;
        public float   Length;
    
        public void Kill()
        {
            Size = -1f;
        }
    }
    
    // FPS
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXHitSparksRequest
    {
        public Vector3 Position;
        public Vector3 Color;
    }*/

    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXSparksRequest
    {
        public Vector3 Position;
        public Vector3 Color;
    }
    
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXSmokeRequest
    {
        public Vector3 Position;
        public float Size;
    }
    
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXBloodHeadshotRequest
    {
        public Vector3 Position;
    }
    
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXBloodSprayRequest
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Angle;
    }
    
    
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
    public struct VFXTrailData : IKillableVFX
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Color;
        public float   Size;
        public float   Length;
        
        public void Kill()
        {
            Size = -1f;
        }
    }
}