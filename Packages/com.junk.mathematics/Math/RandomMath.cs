using System;
using UnityEngine;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Junk.Math
{
    /// <summary>
    /// An inclusive range of values. <see cref="Start"/> should be less than or equal to <see cref="End"/>.
    /// </summary>
    [Serializable]
    public struct Range
    {
        public float Start;
        public float End;

        public Range(float start, float end)
        {
            Start = start;
            End = end;
        }
    }

    [Serializable]
    public struct Range3
    {
        public Range X;
        public Range Y;
        public Range Z;

        public Range3(Range randomVelocityX, Range randomVelocityY, Range randomVelocityZ)
        {
            X = randomVelocityX;
            Y = randomVelocityY;
            Z = randomVelocityZ;
        }
    }
    
    public static class RandomMath
    {
        public static Color RandomColor()
        {
            var color = new Color();
            color.r = Random.value;
            color.g = Random.value;
            color.b = Random.value;
            color.a = Random.value;
            return color;
        }

        public static float Random01(this ref Unity.Mathematics.Random rand)
        {
            return rand.NextFloat(0.0f, 1.0f);
        }
        public static float RandomN1(this ref Unity.Mathematics.Random rand)
        {
            return rand.NextFloat(-1.0f, 1.0f);
        }
        

        public static float RandomRange(this ref Unity.Mathematics.Random rand, Range range)
        {
            return rand.NextFloat(range.Start, range.End);
        }
        
        public static int RandomRangeInt(this ref Unity.Mathematics.Random rand, Range range)
        {
            return rand.NextInt((int)range.Start, (int)range.End);
        }
        
        public static float3 RandomRange3(this ref Unity.Mathematics.Random rand, Range3 range)
        {
            var min = math.min(range.X.Start, math.min(range.Y.Start, range.Z.Start));
            var max = math.max(range.X.End, math.max(range.Y.End, range.Z.End));
            
            return new float3(rand.NextFloat(min, max), rand.NextFloat(min, max), rand.NextFloat(min, max));
        }
    }
}