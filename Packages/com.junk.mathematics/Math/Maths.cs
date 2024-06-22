using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming

namespace Junk.Math
{
    public static partial class maths
    {
        private static readonly float3 zeroVector    = new float3(0.0f, 0.0f, 0.0f);
        private static readonly float3 oneVector     = new float3(1f, 1f, 1f);
        private static readonly float3 upVector      = new float3(0.0f, 1f, 0.0f);
        private static readonly float3 downVector    = new float3(0.0f, -1f, 0.0f);
        private static readonly float3 leftVector    = new float3(-1f, 0.0f, 0.0f);
        private static readonly float3 rightVector   = new float3(1f, 0.0f, 0.0f);
        private static readonly float3 forwardVector = new float3(0.0f, 0.0f, 1f);
        private static readonly float3 backVector    = new float3(0.0f, 0.0f, -1f);
        
        
        private static readonly float2 oneVector2     = new float2(1f, 1f);


        public static float3 zero    => zeroVector;
        public static float3 one     => oneVector;
        public static float3 up      => upVector;
        public static float3 down    => downVector;
        public static float3 left    => leftVector;
        public static float3 right   => rightVector;
        public static float3 forward => forwardVector;
        public static float3 back    => backVector;
        
        public static float2 one2 => oneVector2;
        
        public static float epsilon => math.EPSILON;

        //public static readonly float Epsilon = !MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinDenormal : MathfInternal.FloatMinNormal;
        public static float pi       => math.PI;// 3.141593f;
        public static float infinity => math.INFINITY;        //= float.PositiveInfinity;
        public const  float negativeInfinity = float.NegativeInfinity;
        public const  float deg2Rad          = 0.01745329f;
        public const  float rad2Deg          = 57.29578f;

        
        /// <summary>
        /// Transforms the up vector by a quaternion.
        /// </summary>
        /// <param name="q">The quaternion transformation.</param>
        /// <returns>The upward vector transformed by the input quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Up(quaternion q) { return math.mul(q, up); }  // for compatibility
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Forward(quaternion q) { return math.mul(q, forward); } 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Back(quaternion q) { return math.mul(q, back); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Down(quaternion q) { return math.mul(q, down); } 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Left(quaternion q) { return math.mul(q, left); } 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Right(quaternion q) { return math.mul(q, right); } 
        
        
        
        public static void SetTranslation(this ref float4x4 m, float3 translation)
        {
            m.c3.x = translation.x;
            m.c3.y = translation.y;
            m.c3.z = translation.z;
            
            //Extract Scale
            //For this, take the length of the first three column vectors:
            //var            scale = new float3(math.length(m.c0), math.length(m.c1), math.length(m.c2)); 
            //var           rot   = float4x4.TRS(translation, quaternion.identity, scale);
        }
        
        public static void SetRotation(this ref float4x4 m, quaternion rotation)
        {
            var scale = new float3(math.length(m.c0), math.length(m.c1), math.length(m.c2));
            var translation = m.c3.xyz;
            m = float4x4.TRS(translation, rotation, scale);
        }
        
        /*
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct MathfInternal
        {
            public static volatile float FloatMinNormal = 1.175494E-38f;
            public static volatile float FloatMinDenormal = math.EPSILON;
            public static bool IsFlushToZeroEnabled = (double) MathfInternal.FloatMinDenormal == 0.0;
        }*/
        
       /* dont think it works, should assert and verify
        public static float round(float x, int decimals) {
            return math.round(x * math.pow(10, decimals));
        }*/
        public static float lerpSmooth(float x, float y, float t)
        {
           return (float)math.lerp(x, y, math.smoothstep(0.0, 1.0, math.smoothstep(0.0, 1.0, t)));
        }
        
        public static float CubicLerp(float from, float to, float t)
        {
            float t2 = math.mul(t, t);
            float t3 = math.mul(t2, t);
            return (float)((math.mul(2.0, t3) - 3.0 * t2 + 1.0)*from + (math.mul(-2.0, t3) + math.mul(3.0, t2))*to);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool approximately(float a, float b)
        {
            return math.abs(b - a) < (double) math.max(1E-06f * math.max(math.abs(a), math.abs(b)), epsilon * 8f);
        }
        
        // Compares two floating point values if they are similar.
        // taken from https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Mathf.cs#L279
        public static bool approximately2(float a, float b)
        {
            // If a or b is zero, compare that the other is less or equal to epsilon.
            // If neither a or b are 0, then find an epsilon that is good for
            // comparing numbers at the maximum magnitude of a and b.
            // Floating points have about 7 significant digits, so
            // 1.000001f can be represented while 1.0000001f is rounded to zero,
            // thus we could use an epsilon of 0.000001f for comparing values close to 1.
            // We multiply this epsilon by the biggest magnitude of a and b.
            return math.abs(b - a) < math.max(0.000001f * math.max(math.abs(a), math.abs(b)), math.EPSILON * 8);
        }
        
        public static bool approximately(float3 rhs, float3 lhs)
        {
            return approximately(rhs.x, lhs.x) && approximately(rhs.y, lhs.y) && approximately(rhs.z, lhs.z);
        }
                    // untested brunocoimbra
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public static bool Approximately(float a, float b)
                    {
                        return math.abs(b - a) < (double)math.max(1E-06f * math.max(math.abs(a), math.abs(b)), math.FLT_MIN_NORMAL * 8f);
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public static bool Approximately(double a, double b)
                    {
                        return math.abs(b - a) < math.max(1E-06 * math.max(math.abs(a), math.abs(b)), math.DBL_MIN_NORMAL * 8);
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public static bool Approximately(float a, float b, float tolerance)
                    {
                        return math.abs(a - b) <= tolerance;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public static bool Approximately(double a, double b, double tolerance)
                    {
                        return math.abs(a - b) <= tolerance;
                    }
        
        /// <summary>
        /// equiv of Quaternion.Angle
        /// </summary>
        public static float angle(quaternion a, quaternion b)
        {
            float num = math.dot(a, b);
            
            return IsEqualUsingDot(num) ? 0.0f : (float) ((double) math.acos(math.min(math.abs(num), 1f)) * 2.0 * 57.2957801818848);
        }
        
        /// <summary>
        /// equiv of Quaternion.Angle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEqualUsingDot(float dot)
        {
            return (double) dot > 0.999998986721039;
        }
        
        /// <summary>
        /// Returns the angle between two vectors, replacement for Vector3.Angle
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float angle(float3 lhs, float3 rhs)
        {
            var result =
            
            // see https://answers.unity.com/questions/1294512/how-vectorangle-works-internally-in-unity.html
            math.acos(math.clamp(math.dot(math.normalizesafe(lhs), math.normalizesafe(rhs)), -1f, 1f)) * 57.29578f;
            
            /*var result = math.dot(math.normalizesafe(lhs), math.normalizesafe(rhs));
            result = math.clamp(result, -1f, 1f);
            result = math.acos(result);
            result = math.degrees(result);*/
            
            //var a = Vector3.Angle(lhs, rhs);
            
            //Assert.AreApproximatelyEqual(a,result);

            return result;
        }

        //returns same vector to two decimal places
        public static bool sameVector(Vector3 lhs, Vector3 rhs)
        {
            
            var x = System.Math.Round(lhs.x, 2);
            var y = System.Math.Round(lhs.y, 2);
            var z = System.Math.Round(lhs.z, 2);
            var xyz = new double3(x,y,z);
            
            var a = System.Math.Round(rhs.x, 2);
            var b = System.Math.Round(rhs.y, 2);
            var c = System.Math.Round(rhs.z, 2);
            var abc = new double3(a,b,c);

            return xyz.Equals(abc);
        }
        
        //Returns angle in degree, modulo 360.
        public static float anglemod(float a)
        {
            if (a >= 0)
                a -= 360 * (int) (a / 360);
            else
                a += 360 * (1 + (int) (-a / 360));
            a = (float) 360.0 / 65536 * ((int) (a * (65536 / 360.0)) & 65535);

            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float abs(float value)
        {
            value = (math.abs(value) < epsilon) ? 0.0f : value;
            return value;
        }

        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool select(byte a, bool b, bool c) { return c ? b : a != 0; }

        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool select(bool a, bool b, bool c) { return c ? b : a; }
        
        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity select(Entity a, Entity b, bool c) { return c ? b : a; }
        
        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 select(float4x4 a, float4x4 b, bool c) { return c ? b : a; }
        
        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion select(quaternion a, quaternion b, bool c) { return c ? b : a; }
        
        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 select(float3 a, float3 b, bool c) { return c ? b : a; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 clampMagnitude(float3 vector, float maxLength)
        {
            float sqrMagnitude = math.lengthsq(vector); //vector.sqrMagnitude;
            if ((double) sqrMagnitude <= (double) maxLength * (double) maxLength)
                return vector;
            float num1 = (float) math.sqrt((double) sqrMagnitude);
            float num2 = vector.x / num1;
            float num3 = vector.y / num1;
            float num4 = vector.z / num1;
            return new float3(num2 * maxLength, num3 * maxLength, num4 * maxLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 scale(float3 lhs, float3 rhs)
        {
            return new float3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
        }

        
        /// <summary>
        /// this can be used to snap individual super-small property
        /// values to zero, for avoiding some floating point issues.
        /// </summary>
        public static float3 notnan(float3 value, float newepsilon = 0.0001f) // was SnapToZero
        {
            value.x = (math.abs(value.x) < newepsilon) ? 0.0f : value.x;
            value.y = (math.abs(value.y) < newepsilon) ? 0.0f : value.y;
            value.z = (math.abs(value.z) < newepsilon) ? 0.0f : value.z;
            return value;
        }


        /// <summary>Snaps small numbers or nan to zero, to avoid floating point issues.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float notnan(float value, float newepsilon = 0.0001f)
        {
            value = (math.abs(value) < newepsilon) ? 0.0f : value;
            return value;
        }

        
        /// <summary>Snaps small numbers or nan to zero, to avoid floating point issues.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 snapToZero(float3 value)
        {
            value.x = maths.abs(value.x);
            value.y = maths.abs(value.y);
            value.z = maths.abs(value.z);
            return value;
        }

        // should this be ref?
        /// <summary>Subtract to zero but not less than.</summary>
        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void decrement(ref int x)
        {
            if (x > 0)
                x--;
        }*/

        /// <summary>Subtract to zero but not less than.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int decrement(int x)
        {
            //var truth = x;
            //decrement(ref truth);
            //var val   = math.select(0, x - 1, x > 0);
            //Assert.AreEqual(val, truth);
            
            return math.select(0, x - 1, x > 0);
        }
        
        /*
        public static float SnapToZeroA(float value, float epsilon = 0.0001f)
        {
            value = (Mathf.Abs(value) < epsilon) ? 0.0f : value;
            return value;

        }   */
        /// <summary>
        /// this can be used to snap individual super-small property
        /// values to zero, for avoiding some floating point issues.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float snapToZeroB(float value, float epsilon = 0.0001f)
        {
            value = (math.abs(value) < epsilon) ? 0.0f : value;
            return value;
        }

        /// <summary>
        ///   <para>Projects a vector onto a plane defined by a normal orthogonal to the plane.</para>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="planeNormal"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 projectOnPlane(float3 vector, float3 planeNormal)
        {
            float num1 = math.dot(planeNormal, planeNormal);
            if ((double) num1 < (double) maths.epsilon)
                return vector;
            float num2 = math.dot(vector, planeNormal);
            var result = new float3(vector.x - planeNormal.x * num2 / num1, vector.y - planeNormal.y * num2 / num1, vector.z - planeNormal.z * num2 / num1);
            /*
#if UNITY_EDITOR
            float3 actual = Vector3.ProjectOnPlane(vector, planeNormal);
            Assert.AreEqual(actual, result);
#endif*/
            return result;
            //return math.project(vector, planeNormal); //project is NOT the same as projectonplane
        }
        
        /// <summary>
        /// https://forum.unity.com/threads/rotate-towards-c-jobs.836356/
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxDegreesDelta"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(
            quaternion from,
            quaternion to,
            float      maxDegreesDelta)
        {
            float num = Angle(from, to);
            return num < math.EPSILON ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / num));
        }
         
        /// <summary>
        /// see above
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this quaternion q1, quaternion q2)
        {
            var dot = math.dot(q1, q2);
            return !(dot > 0.999998986721039) ? (float) (math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion nanSafeQuaternion(quaternion quaternion, quaternion prevQuaternion = default(quaternion))
        {
            quaternion.value.x = double.IsNaN(quaternion.value.x) ? prevQuaternion.value.x : quaternion.value.x;
            quaternion.value.y = double.IsNaN(quaternion.value.y) ? prevQuaternion.value.y : quaternion.value.y;
            quaternion.value.z = double.IsNaN(quaternion.value.z) ? prevQuaternion.value.z : quaternion.value.z;
            quaternion.value.w = double.IsNaN(quaternion.value.w) ? prevQuaternion.value.w : quaternion.value.w;

            return quaternion;
        }

        /// <summary>
        /// should be the mathf equiv of LookRotation see https://forum.unity.com/threads/reading-from-localtoworld-and-quaternion-solved.673894/#post-5154296 for details
        /// tldr: use normalized results for something that has scale
        /// </summary>
        public static quaternion LookRotationNormalized(float3 a, float3 b)
        {
            var result = quaternion.LookRotation(math.normalize(a), math.normalize(b));
            return result;
        }

        public static quaternion ToQ(float3 v)
        {
            return ToQ(v.y, v.x, v.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion ToQ(float yaw, float pitch, float roll)
        {
            yaw   *= Mathf.Deg2Rad;
            pitch *= Mathf.Deg2Rad;
            roll  *= Mathf.Deg2Rad;
            float      rollOver2     = roll * 0.5f;
            float      sinRollOver2  = (float) System.Math.Sin((double) rollOver2);
            float      cosRollOver2  = (float) System.Math.Cos((double) rollOver2);
            float      pitchOver2    = pitch * 0.5f;
            float      sinPitchOver2 = (float) System.Math.Sin((double) pitchOver2);
            float      cosPitchOver2 = (float) System.Math.Cos((double) pitchOver2);
            float      yawOver2      = yaw * 0.5f;
            float      sinYawOver2   = (float) System.Math.Sin((double) yawOver2);
            float      cosYawOver2   = (float) System.Math.Cos((double) yawOver2);
            Quaternion result;
            result.w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            result.z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

            return result;
        }

        /// <summary>
        /// https://forum.unity.com/threads/how-do-i-clamp-a-quaternion.370041/
        /// Vector3 bounds = new Vector3(20, 30, 5); // ie: x axis has a range of -20 to 20 degrees
        /// </summary>
        public static quaternion clampQuaternionSafe(quaternion q, float x, float y, float z)
        {
            q.value.x /= q.value.w;
            q.value.y /= q.value.w;
            q.value.z /= q.value.w;
            q.value.w =  1.0f;

            var angleX = 2.0f * Mathf.Rad2Deg * math.atan(q.value.x);
            angleX = math.clamp(angleX, -x, x);
            q.value.x    = math.tan(0.5f * Mathf.Deg2Rad * angleX);
 
            float angleY = 2.0f * Mathf.Rad2Deg * math.atan(q.value.y);
            angleY = math.clamp(angleY, -y, y);
            q.value.y    = math.tan(0.5f * Mathf.Deg2Rad * angleY);
 
            float angleZ = 2.0f * Mathf.Rad2Deg * math.atan(q.value.z);
            angleZ = math.clamp(angleZ, -z, z);
            q.value.z    = math.tan(0.5f * Mathf.Deg2Rad * angleZ);
            
            return nanSafeQuaternion(q);
        }
        //copy this one https://stackoverflow.com/questions/12088610/conversion-between-euler-quaternion-like-in-unity3d-engine/12122899#12122899
        //zardini123 https://forum.unity.com/threads/is-there-a-conversion-method-from-quaternion-to-euler.624007/
        /*
         *   public static quaternion unityEulerToQuaternion(float3 v)
    {
      return unityEulerToQuaternion(v.y, v.x, v.z);
    }
 
    public static quaternion unityEulerToQuaternion(float yaw, float pitch, float roll)
    {
      yaw = math.radians(yaw);
      pitch = math.radians(pitch);
      roll = math.radians(roll);
 
      float rollOver2 = roll * 0.5f;
      float sinRollOver2 = (float)math.sin((double)rollOver2);
      float cosRollOver2 = (float)math.cos((double)rollOver2);
      float pitchOver2 = pitch * 0.5f;
      float sinPitchOver2 = (float)math.sin((double)pitchOver2);
      float cosPitchOver2 = (float)math.cos((double)pitchOver2);
      float yawOver2 = yaw * 0.5f;
      float sinYawOver2 = (float)math.sin((double)yawOver2);
      float cosYawOver2 = (float)math.cos((double)yawOver2);
      float4 result;
      result.w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
      result.x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
      result.y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
      result.z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
 
      return new quaternion(result);
    }
 
    public static float3 unityQuaternionToEuler(quaternion q2)
    {
      float4 q1 = q2.value;
 
      float sqw = q1.w * q1.w;
      float sqx = q1.x * q1.x;
      float sqy = q1.y * q1.y;
      float sqz = q1.z * q1.z;
      float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
      float test = q1.x * q1.w - q1.y * q1.z;
      float3 v;
 
      if (test > 0.4995f * unit)
      { // singularity at north pole
        v.y = 2f * math.atan2(q1.y, q1.x);
        v.x = math.PI / 2;
        v.z = 0;
        return NormalizeAngles(math.degrees(v));
      }
      if (test < -0.4995f * unit)
      { // singularity at south pole
        v.y = -2f * math.atan2(q1.y, q1.x);
        v.x = -math.PI / 2;
        v.z = 0;
        return NormalizeAngles(math.degrees(v));
      }
 
      quaternion q3 = new quaternion(q1.w, q1.z, q1.x, q1.y);
      float4 q = q3.value;
 
      v.y = math.atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));   // Yaw
      v.x = math.asin(2f * (q.x * q.z - q.w * q.y));                                         // Pitch
      v.z = math.atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));   // Roll
 
      return NormalizeAngles(math.degrees(v));
    }
 
    static float3 NormalizeAngles(float3 angles)
    {
      angles.x = NormalizeAngle(angles.x);
      angles.y = NormalizeAngle(angles.y);
      angles.z = NormalizeAngle(angles.z);
      return angles;
    }
 
    static float NormalizeAngle(float angle)
    {
      while (angle > 360)
        angle -= 360;
      while (angle < 0)
        angle += 360;
      return angle;
    }
         */
        /// <summary>
        /// returns the euler angles of a quaternion(tested against UnityEngine.Quaternion.eulerAngles)
        /// </summary>
        /// <param name="q1"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 eulerXYZ(this quaternion rot)
        {
            float4 q1   = rot.value;
            float  sqw  = q1.w * q1.w;
            float  sqx  = q1.x * q1.x;
            float  sqy  = q1.y * q1.y;
            float  sqz  = q1.z * q1.z;
            float  unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float  test = q1.x * q1.w - q1.y * q1.z;
            float3 v;
 
            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.y = 2f * math.atan2(q1.y, q1.x);
                v.x = math.PI / 2f;
                v.z = 0;
                return normalizeAngles(v);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.y = -2f * math.atan2(q1.y, q1.x);
                v.x = -math.PI / 2;
                v.z = 0;
                return normalizeAngles(v);
            }
 
            rot = new quaternion(q1.w, q1.z, q1.x, q1.y);
            v.y = math.atan2(2f * rot.value.x * rot.value.w + 2f * rot.value.y * rot.value.z, 1 - 2f * (rot.value.z * rot.value.z + rot.value.w * rot.value.w));     // Yaw
            v.x = math.asin(2f * (rot.value.x * rot.value.z - rot.value.w * rot.value.y));                             // Pitch
            v.z = math.atan2(2f * rot.value.x * rot.value.y + 2f * rot.value.z * rot.value.w, 1 - 2f * (rot.value.y * rot.value.y + rot.value.z * rot.value.z));      // Roll
            return normalizeAngles(v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 eulerXYZRadians(quaternion q1)
        {
            return math.radians(q1.eulerXYZ());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 normalizeAngles(float3 angles)
        {
            angles.x = normalizeAngle(angles.x);
            angles.y = normalizeAngle(angles.y);
            angles.z = normalizeAngle(angles.z);
            return angles;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float normalizeAngle(float angle)
        {
            while (angle > math.PI * 2f)
                angle -= math.PI * 2f;
            while (angle < 0)
                angle += math.PI * 2f;
            return angle;
        }
        
        /// <summary>
        /// returns a quaternion rotated randomly around a specific axis, typically used for raycast surface normal calculations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion randomAroundAxis(float3 surfaceNormal, ref Unity.Mathematics.Random random)
        {
            // from https://answers.unity.com/questions/1232279/more-specific-quaternionlookrotation.html
            //var x = Quaternion.AngleAxis(random.NextFloat(0, 360f),surfaceNormal) * Quaternion.LookRotation(hit.normal);
            var normal = surfaceNormal;
                
            // Should there be checks for other Unit measurements?
            if (normal.Equals(up) ) 
            {
                normal          += new float3(random.NextFloat(-0.002f, 0.002f), 0, random.NextFloat(-0.002f, 0.002f));
            }
            
            return math.mul(quaternion.AxisAngle(surfaceNormal,random.NextFloat(0, 360f)) , quaternion.LookRotationSafe(normal, up));
        }
        
         // Doesnt appear to work as a static method, need to put into each system
        public static NativeArray<Unity.Mathematics.Random> GetRandoms(Unity.Mathematics.Random random, int count)
        {
            var array = new NativeArray<Unity.Mathematics.Random>(count, Allocator.TempJob);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Unity.Mathematics.Random((uint) random.NextInt());
                
            }

            return array;
        }
        
   
        
        public static float4x4 RotateAround(LocalToWorld localToWorld, float3 center, float3 axis, float angle) {
            var initialRot = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up);
            var rotAmount  = quaternion.AxisAngle(axis, angle);
            var finalPos   = center + math.mul(rotAmount, localToWorld.Position - center);
            var finalRot   = math.mul(math.mul(initialRot, math.mul(math.inverse(initialRot), rotAmount)), initialRot);
            return new float4x4(finalRot, finalPos);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPositive(this int number)
        {
            return number > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNegative(this int number)
        {
            return number < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this int number)
        {
            return number.Equals(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAwesome(this int number)
        {
            return IsNegative(number) && IsPositive(number) && IsZero(number);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPositive(this float number)
        {
            return number > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNegative(this float number)
        {
            return number < 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this float number)
        {
            return number.Equals(0);
        }


        public static void GetArcHits(out List<RaycastHit2D> Hits, out List<Vector3> Points,
            int                                              iLayerMask,
            Vector3                                          vStart,        Vector3 vVelocity,
            Vector3                                          vAcceleration, float   fTimeStep = 0.05f, float fMaxtime = 10f,
            bool                                             bIncludeUnits = false,
            bool                                             bDebugDraw    = false)
        {
            Hits   = new List<RaycastHit2D>();
            Points = new List<Vector3>();

            Vector3 prev = vStart;
            Points.Add(vStart);

            for (int i = 1;; i++)
            {
                float t = fTimeStep * i;
                if (t > fMaxtime) break;
                Vector3 pos = PlotTrajectoryAtTime(vStart, vVelocity, vAcceleration, t);

                var result = Physics2D.Linecast(prev, pos, iLayerMask);
                if (result.collider != null)
                {
                    Hits.Add(result);
                    Points.Add(pos);
                    break;
                }
                else
                {
                    Points.Add(pos);
                }

                Debug.DrawLine(prev, pos, Color.Lerp(Color.yellow, Color.red, 0.35f), 0.5f);

                prev = pos;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, Vector3 acceleration,
            float                                          fTimeSinceStart)
        {
            return start + startVelocity * fTimeSinceStart + acceleration * fTimeSinceStart * fTimeSinceStart * 0.5f;
        }
        
        public static Vector3 ToVector3(this Vector4 parent)
        {
            return new Vector3(parent.x, parent.y, parent.z);
        }

 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 WorldToLocal(this float4x4 transform, float3 point)
        {
            return math.transform(math.inverse(transform), point);
        }
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 LocalToWorld(this float4x4 transform, float3 point)
        {
            return math.transform(transform, point);
        }



        // is this already here?
        //todo change to float2
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NaNSafeVector2(Vector2 vector, Vector2 prevVector = default(Vector2))
        {
            vector.x = double.IsNaN(vector.x) ? prevVector.x : vector.x;
            vector.y = double.IsNaN(vector.y) ? prevVector.y : vector.y;

            return vector;
        }

        #region Transform

        public static float3 RotateAroundPoint(float3 position, float3 pivot, float3 axis, float delta)
        {
            return math.mul(quaternion.AxisAngle(axis, delta), position - pivot) + pivot;
        }

        public static float3 InverseTransformPoint(float3 point, float4x4 transform)
        {
            //var ltw             = new LocalToWorld();
            var position        = PositionFromMatrix(transform);
            var inverseRotation = math.inverse(new quaternion(transform));
            var result          = point - position;
            result = math.mul(inverseRotation,result);
            
            var s  =  Vector3.Scale(one, math.mul(inverseRotation, result));
            //scale()

            return s;
        }

        #endregion

        #region Random
        
        /// <summary>
        /// see https://forum.unity.com/threads/random-insideunitsphere-circle.920045/#post-6023861
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 insideSphere(this ref Unity.Mathematics.Random rand)
        {
            var phi   = rand.NextFloat(2 * math.PI);
            var theta = math.acos(rand.NextFloat(-1f, 1f));
            var r     = math.pow(rand.NextFloat(), 1f / 3f);
            var x     = math.sin(theta) * math.cos(phi);
            var y     = math.sin(theta) * math.sin(phi);
            var z     = math.cos(theta);
            return r * new float3(x, y, z);
        }
 
        public static float3 onSphereSurface(this ref Unity.Mathematics.Random rand)
        {
            var phi   = rand.NextFloat(2 * math.PI);
            var theta = math.acos(rand.NextFloat(-1f, 1f));
            var x     = math.sin(theta) * math.cos(phi);
            var y     = math.sin(theta) * math.sin(phi);
            var z     = math.cos(theta);
            return new float3(x, y, z);
        }
        
        public static float3 groundSplatterVector(ref Unity.Mathematics.Random random)
        {
            //var pseudoRandomVector = ;
            return new float3(random.NextFloat(-0.50f, 0.50f), -random.NextFloat(0, 1), random.NextFloat(-0.50f, 0.50f));
        }
        
        #endregion

        
        #region Transform Matrix Helpers

        public static float3 RightFromMatrix(float4x4 Value) => new float3(Value.c0.x, Value.c0.y, Value.c0.z);
        
        public static float3 UpFromMatrix(float4x4 Value) => new float3(Value.c1.x, Value.c1.y, Value.c1.z);
        
        public static float3 ForwardFromMatrix(float4x4 Value) => new float3(Value.c2.x, Value.c2.y, Value.c2.z);
        
        public static float3 PositionFromMatrix(float4x4 Value) => new float3(Value.c3.x, Value.c3.y, Value.c3.z);

        public static quaternion RotationFromMatrix(float4x4 Value) => new quaternion(math.orthonormalize(new float3x3(Value))); // old new quaternion(Value);
        
        #endregion

        /// <summary>
        /// gets a Getting perpendicular direction vector from surface normal
        /// https://answers.unity.com/questions/722531/getting-perpendicular-direction-vector-from-surfac.html
        /// </summary>
        public static float3 parallel(float3 direction, float3 surfaceNormal)
        {
            // add test case for this
            //Vector3 surfaceParallel = direction - surfaceNormal * Vector3.Dot(direction, surfaceNormal);
            
            var surfaceParallel = direction - surfaceNormal * math.dot(direction, surfaceNormal);
            
            return surfaceParallel;
        }
        
        /// <summary>
        /// credit to hippocoder - https://forum.unity.com/threads/lerp-multi-points-a-to-b-to-c.453751/
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 lerp(float3 a, float3 b, float3 c, float t)
        {
            return t <= 0.5f ? math.lerp(a, b, t * 2f) : math.lerp(b * 2f, c, t);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 lerp(NativeArray<float3> array, float t)
        {
            return t <= 0.5f ? math.lerp(array[0], array[1], t * 2f) : math.lerp(array[1], array[2], t);
        }

        /// <summary>
        /// credit to hippocoder - https://forum.unity.com/threads/lerp-multi-points-a-to-b-to-c.453751/
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float lerp(float a, float b, float c, float t)
        {
            return t <= 0.5f ? math.lerp(a, b, t * 2f) : math.lerp(b, c, t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion slerp(quaternion q1, quaternion q2, quaternion q3, float t)
        {
            return t <= 0.5f ? math.slerp(q1, q2, t * 2f) : math.slerp(q2, q3, (t - 0.5f) * 2f);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion slerp(NativeArray<quaternion> array, float t)
        {
            return t <= 0.5f ? math.slerp(array[0], array[1], t * 2f) : math.slerp(array[1], array[2], (t - 0.5f) * 2f);
        }
        
        public static bool equals(int a, int b, int c)
        {
            return a == b && a == c;
        }
        
        
        /// <summary>Rescales a float4x4 scale matrix given 3 axis scales.</summary>
        /// <param name="s">The uniform scaling factor.</param>
        /// <returns>The float4x4 matrix that represents a uniform scale.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 rescale(this ref float4x4 matrix, float s)
        {
            matrix.c0.x = s;
            matrix.c1.y = s;
            matrix.c2.z = s;
            return matrix;
        }

        /// <summary>Rescales a float4x4 scale matrix given 3 axis scales.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 rescale(this ref float4x4 matrix, float x, float y, float z)
        {
            matrix.c0.x = x;
            matrix.c1.y = y;
            matrix.c2.z = z;
            return matrix;
        }

        /// <summary>Returns a float4x4 scale matrix given a float3 vector containing the 3 axis scales.</summary>
        /// <param name="scales">The vector containing scale factors for each axis.</param>
        /// <returns>The float4x4 matrix that represents a non-uniform scale.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4x4 rescale(this ref float4x4 matrix, float3 scales)
        {
            matrix.c0.x = scales.x;
            matrix.c1.y = scales.y;
            matrix.c2.z = scales.z;
            return matrix;
        }
        
        // is this correct? - taken from retrofunctions
        public static float remap(float x, float in_min, float in_max, float out_min, float out_max)
        {
            float t = (x - in_min) / (in_max - in_min);
            return math.lerp(out_min, out_max, t);
        }

        public static float remap_clamp(float x, float in_min, float in_max, float out_min, float out_max)
        {
            float t = (x - in_min) / (in_max - in_min);
            t = math.clamp(t, 0, 1);
            return math.lerp(out_min, out_max, t);
        }
        
        /// <summary>
        ///   <para>Calculates the angle between vectors from and.</para>
        /// </summary>
        /// <param name="from">The vector from which the angular difference is measured.</param>
        /// <param name="to">The vector to which the angular difference is measured.</param>
        /// <returns>
        ///   <para>The angle in degrees between the two vectors.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float angle(Vector3 from, Vector3 to)
        {
            float num = (float) math.sqrt((double) from.sqrMagnitude * (double) to.sqrMagnitude);
            return (double) num < 1.0000000036274937E-15 ? 0.0f : (float) math.acos((double) math.clamp(math.dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float signedAngle(float3 from, float3 to, float3 axis)
        {
            float num1 = angle(from, to);
            float num2 = (float) ((double) from.y * (double) to.z - (double) from.z * (double) to.y);
            float num3 = (float) ((double) from.z * (double) to.x - (double) from.x * (double) to.z);
            float num4 = (float) ((double) from.x * (double) to.y - (double) from.y * (double) to.x);
            float num5 = math.sign((float) ((double) axis.x * (double) num2 + (double) axis.y * (double) num3 + (double) axis.z * (double) num4));
            return num1 * num5;
        }
        
        /// <summary>
        /// Sets a float3's components to the positive or negative of another float3.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="otherVector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 real(float3 value, float3 otherVector)
        {
            value.x = math.select(value.x, -value.x, otherVector.x.IsNegative());
            value.y = math.select(value.y, -value.y, otherVector.y.IsNegative());
            value.z = math.select(value.z, -value.z, otherVector.z.IsNegative());
            
            /*value.x = otherVector.x < 0.0f ? -value.x : value.x;
            value.y = otherVector.y < 0.0f ? -value.y : value.y;
            value.z = otherVector.z < 0.0f ? -value.z : value.z;*/
            return value;
        }
    }
}