using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Junk.Cameras
{
    public struct ThirdPersonCamera : IComponentData
    {
        public bool           Enabled;
        public Entity         Target;
        public float3         Distance;
        public bool           SnapToLookAt;
        public CameraSnapMode CameraSnapMode;
        public float          Angle;
        public float          YawUnclamped;
        public float          PreviousYaw;
        public float          Diff;
        public float          DOT;
        public bool           ForceUpdateCameraTransform;
        public float3         Position;
        public quaternion     Rotation;
        public RigidTransform Transform;
        public float3         EulerAngles; // radians
    }
    
    public enum CameraSnapMode
    {
        SnapThenLerp,
        AlwaysSnap,
        Lerp
    }

    public struct PlayerCamera : IComponentData
    {
        public                                      bool           Enabled;
        [FormerlySerializedAs("CameraMode")] public CameraViewMode CameraViewMode;
    }
    
    public struct ThirdPersonCameraSprings : IComponentData
    {
        public Entity PositionSpring;
        public Entity PositionSpring2;
        public Entity RotationSpring;
        public float  PositionSpringStiffness;  //  = 0.7f;
        public float  PositionSpringDamping;    //    = 0.65f;
        public float  PositionSpring2Stiffness; // = 0.95f;
        public float  PositionSpring2Damping;   //   = 0.25f;
        public float  RotationSpringStiffness;
        public float  RotationSpringDamping;
    }

    public struct ThirdPersonCameraSettings : IComponentData
    {
        public float Speed;
        public float PitchSpeed;
        public float YawSpeed;
    }
}