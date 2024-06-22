using Unity.Entities;

namespace Junk.Cameras
{
    /// <summary>
    /// Used by the third person camera to determine the target position of the camera.
    /// </summary>
    public struct CameraTarget : IComponentData
    {
        public Entity Target;
        public Entity TargetEntity;
    }
}