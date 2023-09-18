using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Cameras
{
    /// <summary>
    /// A component with the current recoil value, passed by a weapon to the camera
    /// </summary>
    public struct CameraRecoil : IComponentData
    {
        public int  CooldownFrames; // 
        public bool WeaponIsFiring; // move to separate component
        
        public byte   RecoilFlag; // 1 for has recoil that the camera system needs to react to. 
        public float3 DeltaValue;
        public float3 Total; // need to make this into buffer i think, doesnt really store the total 
    }
}