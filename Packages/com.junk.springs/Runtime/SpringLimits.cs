using Unity.Mathematics;

namespace Junk.Springs
{
    /// <summary>
    /// Immutable blob data 
    /// </summary>
    public struct SpringLimits
    {
        // transform limitations
        public float  MaxVelocity;// = 10000.0f;
        public float  MinVelocity;// = 0.0000001f;
        public float3 MaxState;// = new float3(10000,  10000,  10000);
        public float3 MinState;// = new float3(-10000, -10000, -10000);

    }
    
}