using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace CyberJunk.Procedural
{
    public static class Procedural
    {
        
    }

    public static class Rules
    {
        /// <summary>
        /// Gets a float number usually between 0 and 1.0  //f0.01f and 0.19f
        /// </summary>
        /// <param name="position">position to use</param>
        /// <param name="seed">unique seed</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FloatRange(float3 position, int seed)
        {
            var x       = position * seed + new float3(0.01f,0.01f,0.01f); // if the value is a whole number, offset slightly as whole numbers cause the result to be zero(not useful)
            int hashPos = (int) math.hash(new int3(math.floor(x)));

            var y = noise.cnoise(x);
            y *= 10;
            y *= 5;
            //var nansafe = maths.notnan(y);
            //y = math.abs(y);
            return y;
        }
    }
}