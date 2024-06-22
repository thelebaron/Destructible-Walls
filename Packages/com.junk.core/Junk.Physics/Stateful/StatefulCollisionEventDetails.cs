using Unity.Entities;

namespace Junk.Physics.Stateful
{
    public struct StatefulCollisionEventDetails : IComponentData
    {
        public bool CalculateDetails;
    }
}