using Junk.Math;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Cameras
{
    public struct FirstPersonCamera : IComponentData, IEnableableComponent
    {
        public bool   Enabled;
        public float3 PositionOffset;
        public float3 CrouchOffset;
        // camera roll
        public float RotationStrafeRoll;// = 0.01f;
        // camera bob
        public float4 BobRate;// = new Vector4(0.0f,      1.4f,             0.0f, 0.0f/*0.7f*/); // TIP: use x for a mech / dino like walk cycle. y should be (x * 2) for a nice classic curve of motion. typical defaults for y are 0.9 (rate) and 0.1 (amp)
        public float4 BobAmplitude;// = new Vector4(0.0f, 0.0025f/*0.25f*/, 0.0f, 0.5f);
        public float BobInputVelocityScale;// = 1.0f;
        public float BobMaxInputVelocity;// = 100; // TIP: calibrate using 'Debug.Log(Controller.velocity.sqrMagnitude);'
        public bool BobRequireGroundContact;// = true;
        public float LastBobSpeed;
        public float4 CurrentBobAmp;
        public float4 CurrentBobVal;
        public float BobSpeed;
        // camera bob step variables
        //public delegate void BobStepDelegate();
        //public BobStepDelegate BobStepCallback;
        public float BobStepThreshold;// = 10.0f;
        public float LastYBob;
        public bool  m_BobWasElevating;
        
        // for temporary disabling of specific procedural camera motions at runtime,
        //  bypassesing states (intended to be set directly by VR mode)
        public bool MuteRoll;
        public bool MuteBob;
        public bool MuteShakes;
        public bool MuteEarthquakes;
        public bool MuteBombShakes;
        public bool MuteFallImpacts;
        public bool MuteHeadImpacts;
        public bool MuteGroundStomps;
        // camera rotation
        public bool   SnapSprings;
        public Entity PositionSpring;
        public Entity PositionSpring2;
        public Entity RotationSpring;
        public Entity RecoilSpring;
        public float  PositionSpringStiffness;  //  = 0.7f;
        public float  PositionSpringDamping;    //    = 0.65f;
        public float  PositionSpring2Stiffness; // = 0.95f;
        public float  PositionSpring2Damping;   //   = 0.25f;
        public float  RotationSpringStiffness;
        public float  RotationSpringDamping;
        public float  RecoilSpringStiffness;
        public float  RecoilSpringDamping;
        
        // not user set, forces sent by shooter component to affect springs
        // maybe need to split into separate component data
        public float3 ExternalPositionalForce;
        public float3 ExternalPositionalForce2;
        public float3 ExternalAngularForce;

        public float3 Euler;

        //public float3 AdditiveEulerXYZ; // extra rotation from spring recoil merging

        public void AddForce(float3 positionalForce)
        {
            ExternalPositionalForce += positionalForce;
        }
        
        public void AddForce2(float3 positionalForce)
        {
            ExternalPositionalForce2 += positionalForce;
        }
        public void AddTiltForce(float3 angularForce)
        {
            ExternalAngularForce += maths.right * angularForce;
        }
    }
}