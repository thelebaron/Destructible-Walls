using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Cameras
{
    public struct FirstPersonCameraRecoilXYZ : IComponentData
    {
        public float3 Value;
    }
}