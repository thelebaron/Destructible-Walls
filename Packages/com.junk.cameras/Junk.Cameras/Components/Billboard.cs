using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Cameras
{
    public struct Billboard : IComponentData
    {
        public bool3 Axis;
    }
}