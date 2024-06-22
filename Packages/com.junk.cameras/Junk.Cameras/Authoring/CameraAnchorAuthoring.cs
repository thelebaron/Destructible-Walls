
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Cameras
{
    // Fixed anchor for the camera to follow
    // This is a rigid hierachy connection
    public class CameraAnchorAuthoring : MonoBehaviour
    {
        
    }

    public class CameraAnchorBaker : Baker<CameraAnchorAuthoring>
    {
        public override void Bake(CameraAnchorAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<CameraAnchor>(entity);
        }
    }


}