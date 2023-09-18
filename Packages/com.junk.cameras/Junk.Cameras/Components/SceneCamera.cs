using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Cameras
{
    public struct SceneCamera : IComponentData, IEnableableComponent
    {
        public Rect MouseCursorRect;
        public Rect ScreenCenterRect;
        public bool MouseButtonUp0;
        public bool MouseButtonUp1;
        public bool MouseButtonUp2;
        public bool MouseButtonDown0;
        public bool MouseButtonDown1;
        public bool MouseButtonDown2;

        public bool   Orbit;
        public float3 Pivot;
        public float  Pitch;
        public float  Yaw;
        public float2 Delta;
        public float2 PreviousMousePosition;
        public float  DistanceToCamera;
    }
}