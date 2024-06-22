// todo split into PlayerWeaponCommands and PlayerMovementCommands

using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    public struct PlayerCommands : IComponentData
    {
        public bool   Console;
        public float2 Move;
        public bool   Jump;
        public bool JumpPressedThisFrame;
        public int    JumpHeldTime;
        public bool   LedgeGrabDebug;
        public bool   JumpReleased;
        public bool   Run;
        public bool   RunLastFrame;
        public bool   Crouch;
        public bool   CrouchLastFrame;
        public bool   CrouchHeld;
        public bool   StartSlide;
        public int    SlideTimer;
        public bool   Kill;
        public bool   Respawn; //todo 22.01.21

        public float JumpCooldown;

        // interaction
        public bool Interact;
        public Entity InteractEntity;
        
        // Weapon interaction
        public bool  Drop;
        public bool  CanDrop;
        public float KeyCooldown;
        
        public bool Num1;
        public bool Num2;
        public bool Num3;
        public bool Num4;
        public bool SelectWeapon4;
        public bool SelectWeapon5;
        public bool SelectWeapon6;
        public bool SelectWeapon7;
        public bool SelectWeapon8;
        public bool SelectWeapon9;
        public bool SelectWeapon0;
        public bool SelectNextWeapon;
        public bool SelectPrevWeapon;
        public bool Fire;
        public bool FireDown;
        public bool FireRelease;
        public bool AimDownSights;
        public bool Reload;
        public bool HolsterToggle;
        public bool HolsterState;

        //public float2 MouseLookRawMove;
        public bool MouseLock;
        // Duplicated for more professional feel
        public bool Mouse0;
        public bool Mouse1;
        public bool EditorQuit;
        
        public bool   MouseLookInvert;
        public bool   MouseLookAcceleration;
        public float  MouseLookAccelerationThreshold;
        public float2 MouseLookSensitivity;
        public float2 CurrentMouseLook;
        public int    MouseLookSmoothSteps;
        public float  MouseLookSmoothWeight;
        public int    LastMouseLookFrame;
        public float2 MouseLookSmoothMove;
        public float2 CumulativeMouseLook;
        public float2 MouseLookRawMove;

        public float Yaw; // total cumulative yaw, clamped
        public float Pitch; // total cumulative pitch, clamped
        
        public float2 RotationYawLimit;
        public float2 RotationPitchLimit;
        public float2 RawMouseLook;
        
        // Tank controls
        public float TankYaw;
        public float TankForward;


        /// <summary>
        /// Clamps the pitch and yaw within reasonable(but not yet configurable) amounts. Allows modifying yaw and pitch.
        /// </summary>
        public PlayerCommands ClampPitchYawAngles(PlayerCommands input, float yawAngle = 0, float pitchAngle = 0)
        {
            var rotationPitchLimit = new float2(90.0f, -90.0f);
            var rotationYawLimit   = new float2(-360.0f, 360.0f);
            
            input.Yaw   += yawAngle;
            input.Pitch += pitchAngle;
            
            // clamp angles
            input.Yaw   = input.Yaw < -360.0f ? input.Yaw += 360.0f : input.Yaw;
            input.Yaw   = input.Yaw > 360.0f ? input.Yaw  -= 360.0f : input.Yaw;
            input.Yaw   = math.clamp(input.Yaw, rotationYawLimit.x, rotationYawLimit.y);
            input.Pitch = input.Pitch < -360.0f ? input.Pitch += 360.0f : input.Pitch;
            input.Pitch = input.Pitch > 360.0f ? input.Pitch  -= 360.0f : input.Pitch;
            input.Pitch = math.clamp(input.Pitch, -rotationPitchLimit.x, -rotationPitchLimit.y);

            return input;
        }
    }
    
    public static class PlayerCommandsUtility
    {
        public static PlayerCommands TankControlsClampYaw(this PlayerCommands input, float yaw)
        {
            input.Yaw = ClampYaw(yaw);
            return input;
        }
        
        public static float ClampYaw(float yaw)
        {
            yaw = yaw < -360.0f ? yaw += 360.0f : yaw;
            yaw = yaw > 360.0f ? yaw  -= 360.0f : yaw;
            return math.clamp(yaw, -360.0f, 360.0f);
        }
    }
}
