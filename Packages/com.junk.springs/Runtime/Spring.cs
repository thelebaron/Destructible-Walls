using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace Junk.Springs
{
    /// <summary>
    ///     Spring component. Note constructor should always be used unless a blob is explicitly created for it from the
    ///     SpringUtility class.
    ///     notes: spring should be separated from the spring state, theres a lot of internal data that shouldnt be exposed to
    ///     a user.
    /// </summary>
    public struct Spring : IComponentData, IEnableableComponent
    {
        public bool   Enabled;
        public float3 RestState; // the static equilibrium of this spring
        public float3 Stiffness; // mechanical strength of the spring - default = float3(0.5f, 0.5f, 0.5f);
        public float3 Damping; // 'Damping' makes spring velocity wear off as it approaches its rest state default = new float3(0.75f, 0.75f, 0.75f);
        public bool Stop;
        public  bool StopAndIncludeSoftForce;

        // unity bug - modifying blob authoring in editor at runtime causes errors
        public BlobAssetReference<SpringLimits> SpringLimitsReference;
        
        public Spring(float3 restState = new(), float minVelocity = 0.000001f, bool enabled = true)
        {
            Value                     = 0;
            PreviousValue             = 0;
            Delta                     = 0;
            Velocity                  = 0;
            RestState                 = restState;
            Stiffness                 = new float3(0.5f, 0.5f, 0.5f);
            Damping                   = new float3(0.75f, 0.75f, 0.75f);
            VelocityFadeInCap         = 1f;
            VelocityFadeInEndTime     = 0f;
            VelocityFadeInLength      = 0f;
            InternalForce           = 0;
            InternalSoftForce       = 0;
            InternalFrames          = 0;
            Stop                    = false;
            StopAndIncludeSoftForce = false;
            SpringLimitsReference     = SpringBlobUtility.GetSpringBlobData(minVelocity);
            Enabled                   = enabled;
        }

        public float3 Value; // the springs current state
        public float3 PreviousValue;
        public float3 Delta;
        public float3 Velocity;
        
        // force velocity fadein variables
        public float VelocityFadeInCap; // = 1.0f;
        public float VelocityFadeInEndTime; // = 0.0f;
        public float VelocityFadeInLength; // = 0.0f;

        // internal forces
        public float3 InternalForce;
        public float3 InternalSoftForce;
        public int    InternalFrames;

        //todo remove this method
        public void AddForce(float3 force)
        {
            InternalForce += force;
        }

        //todo remove this method
        public void AddSoftForce(float3 force, int frames)
        {
            InternalFrames    =  frames;
            InternalSoftForce += force;
        }

        public void StopSpring(bool includeSoftForce = false)
        {
            Stop                    = true;
            StopAndIncludeSoftForce = includeSoftForce;
        }
    }
}