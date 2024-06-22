using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Cameras
{
    /// <summary>
    /// The entity attached directly to the GameObject tagged as MainCamera or the first returned Camera.
    /// </summary>
    /// <summary>
    /// The Main Camera as an entity. Use this tag to find the camera in scenes, but do not add or remove it.
    /// </summary>
    public struct MainCameraData : IComponentData
    {
        public bool         Orthographic;
        public float3       NearClipPlane;
        public LocalToWorld LocalToWorldRO;
        /*
        public Ray ScreenToWorldPoint(Vector3 screenPos, float fieldOfView = 60, bool orthographic = false) 
        {
            // Remap so (0, 0) is the center of the window,
            // and the edges are at -0.5 and +0.5.
            var relative = new Vector2(
                screenPos.x / Screen.width - 0.5f,
                screenPos.y / Screen.height - 0.5f
            );

            if (!orthographic) {
                // Angle in radians from the view axis
                // to the top plane of the view pyramid.
                float verticalAngle = 0.5f * Mathf.Deg2Rad * fieldOfView;

                // World space height of the view pyramid
                // measured at 1 m depth from the camera.
                float worldHeight = 2f * math.tan(verticalAngle);

                // Convert relative position to world units.
                Vector3 worldUnits = relative * worldHeight;
                worldUnits.x *= aspect;
                worldUnits.z =  1;

                // Rotate to match camera orientation.
                Vector3 direction = transform.rotation * worldUnits;

                // Output a ray from camera position, along this direction.
                return new Ray(transform.position, direction);
            } else {
                // Scale using half-height of camera.
                Vector3 worldUnits = relative * orthographicSize * 2f;
                worldUnits.x *= aspect;

                // Orient and position to match camera transform.
                Vector3 origin = transform.rotation * worldUnits;
                origin += transform.position;
          
                // Output a ray from this point, along camera's axis.
                return new Ray(origin, transform.forward);
            }
        }*/
    }
}