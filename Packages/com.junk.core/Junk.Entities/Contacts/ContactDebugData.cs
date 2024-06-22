using Unity.Entities;
using Unity.Physics;

namespace Junk.Entities
{
    public struct ContactDebugData : IComponentData
    {
        public RaycastHit RaycastHit;
    }
}