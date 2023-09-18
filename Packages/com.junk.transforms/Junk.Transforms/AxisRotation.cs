using Unity.Entities;

namespace Junk.Transforms
{
    /// <summary>
    /// This allows an entity to rotate on its own, using the Presentation system to update its rotation.
    /// </summary>
    public struct AxisRotation: IComponentData
    {
        public float SpeedX;
        public float SpeedY;
        public float SpeedZ;
    }
}