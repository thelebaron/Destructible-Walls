using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Cameras
{
    public struct FPCameraRecoil : IComponentData
    {
        public quaternion Value;
    }
}