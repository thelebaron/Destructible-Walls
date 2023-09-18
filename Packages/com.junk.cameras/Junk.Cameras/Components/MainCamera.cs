using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Cameras
{
    /// <summary>
    /// The entity attached directly to the GameObject tagged as MainCamera or the first returned Camera.
    /// </summary>
    /// <summary>
    /// The Main Camera as an entity. Use this tag to find the camera in scenes, but do not add or remove it.
    /// notes: todo make SystemStateComponent to allow for creation/destruction of the camera entity, and then reinitialize it
    /// </summary>
    public struct MainCamera : IComponentData//, IEnableableComponent
    {
        public bool         Orthographic;
        public float3       NearClipPlane;
        public LocalToWorld LocalToWorldRO;
    }
}