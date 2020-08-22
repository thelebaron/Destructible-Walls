using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming

namespace thelebaron.mathematics
{
    public static class maths
    {
        private static readonly float3 zeroVector    = new float3(0.0f, 0.0f, 0.0f);
        private static readonly float3 oneVector     = new float3(1f, 1f, 1f);
        private static readonly float3 upVector      = new float3(0.0f, 1f, 0.0f);
        private static readonly float3 downVector    = new float3(0.0f, -1f, 0.0f);
        private static readonly float3 leftVector    = new float3(-1f, 0.0f, 0.0f);
        private static readonly float3 rightVector   = new float3(1f, 0.0f, 0.0f);
        private static readonly float3 forwardVector = new float3(0.0f, 0.0f, 1f);
        private static readonly float3 backVector    = new float3(0.0f, 0.0f, -1f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Up(quaternion q) { return math.mul(q, new float3(0, 1, 0)); }  // for compatibility

        public static float3 zero    => zeroVector;
        public static float3 one     => oneVector;
        public static float3 up      => upVector;
        public static float3 down    => downVector;
        public static float3 left    => leftVector;
        
        /// <summary>
        /// Shorthand for postive X
        /// </summary>
        public static float3 right   => rightVector;
        public static float3 forward => forwardVector;
        public static float3 back    => backVector;

        public static float epsilon => math.EPSILON; //0.0001f;

        //public static readonly float Epsilon = !MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinDenormal : MathfInternal.FloatMinNormal;
        public static float pi       => math.PI;// 3.141593f;
        public static float infinity => math.INFINITY;        //= float.PositiveInfinity;
        public const  float negativeInfinity = float.NegativeInfinity;
        public const  float deg2Rad          = 0.01745329f;
        public const  float rad2Deg          = 57.29578f;

        /*
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct MathfInternal
        {
            public static volatile float FloatMinNormal = 1.175494E-38f;
            public static volatile float FloatMinDenormal = float.Epsilon;
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
        
        
        public static bool approximately(float a, float b)
        {
            return math.abs(b - a) < (double) math.max(1E-06f * math.max(math.abs(a), math.abs(b)), epsilon * 8f);
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

        public static float abs(float value)
        {
            value = (math.abs(value) < epsilon) ? 0.0f : value;
            return value;
        }


        /// <summary>Returns b if c is true, a otherwise.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool select(bool a, bool b, bool c)
        {
            return c ? b : a;
        }
        
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

        //[BurstCompile]
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
            /*float num1 = math.dot(planeNormal, planeNormal);
            if ((double) num1 < (double) maths.epsilon)
                return vector;
            float num2 = math.dot(vector, planeNormal);
            var result = new float3(vector.x - planeNormal.x * num2 / num1, vector.y - planeNormal.y * num2 / num1, vector.z - planeNormal.z * num2 / num1);
            */
/*#if UNITY_EDITOR
            float3 actual = Vector3.ProjectOnPlane(vector, planeNormal);
            Assert.AreEqual(actual, result);
#endif*/
            //return result;
            return math.project(vector, planeNormal);
        }

        [BurstCompile]
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

        public static quaternion ToQ(float yaw, float pitch, float roll)
        {
            yaw   *= Mathf.Deg2Rad;
            pitch *= Mathf.Deg2Rad;
            roll  *= Mathf.Deg2Rad;
            float      rollOver2     = roll * 0.5f;
            float      sinRollOver2  = (float) Math.Sin((double) rollOver2);
            float      cosRollOver2  = (float) Math.Cos((double) rollOver2);
            float      pitchOver2    = pitch * 0.5f;
            float      sinPitchOver2 = (float) Math.Sin((double) pitchOver2);
            float      cosPitchOver2 = (float) Math.Cos((double) pitchOver2);
            float      yawOver2      = yaw * 0.5f;
            float      sinYawOver2   = (float) Math.Sin((double) yawOver2);
            float      cosYawOver2   = (float) Math.Cos((double) yawOver2);
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
        
        /// <summary>
        /// returns the euler angles of a quaternion(tested against UnityEngine.Quaternion.eulerAngles)
        /// </summary>
        /// <param name="q1"></param>
        /// <returns></returns>
        public static float3 eulerXYZ(quaternion q1)
        {
            float   sqw  = q1.value.w * q1.value.w;
            float   sqx  = q1.value.x * q1.value.x;
            float   sqy  = q1.value.y * q1.value.y;
            float   sqz  = q1.value.z * q1.value.z;
            float   unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float   test = q1.value.x * q1.value.w - q1.value.y * q1.value.z;
            Vector3 v;

            if (test > 0.4995f * unit)
            {
                // singularity at north pole
                v.y = 2f * Mathf.Atan2(q1.value.y, q1.value.x);
                v.x = Mathf.PI / 2;
                v.z = 0;
                return normalizeAngles(v * Mathf.Rad2Deg);
            }

            if (test < -0.4995f * unit)
            {
                // singularity at south pole
                v.y = -2f * Mathf.Atan2(q1.value.y, q1.value.x);
                v.x = -Mathf.PI / 2;
                v.z = 0;
                return normalizeAngles(v * Mathf.Rad2Deg);
            }

            quaternion q = new quaternion(q1.value.w, q1.value.z, q1.value.x, q1.value.y);
            v.y = (float) Math.Atan2(2f * q.value.x * q.value.w + 2f * q.value.y * q.value.z, 1 - 2f * (q.value.z * q.value.z + q.value.w * q.value.w)); // Yaw
            v.x = (float) Math.Asin(2f * (q.value.x * q.value.z - q.value.w * q.value.y));                                                               // Pitch
            v.z = (float) Math.Atan2(2f * q.value.x * q.value.y + 2f * q.value.z * q.value.w, 1 - 2f * (q.value.y * q.value.y + q.value.z * q.value.z)); // Roll
            return normalizeAngles(v * Mathf.Rad2Deg);
        }

        public static float3 eulerXYZRadians(quaternion q1)
        {
            return math.radians(eulerXYZ(q1));
        }

        static float3 normalizeAngles(float3 angles)
        {
            angles.x = normalizeAngle(angles.x);
            angles.y = normalizeAngle(angles.y);
            angles.z = normalizeAngle(angles.z);
            return angles;
        }

        static float normalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }
        
        /// <summary>
        /// returns a quaternion rotated randomly around a specific axis, typically used for raycast surface normal calculations
        /// </summary>
        public static quaternion randomAroundAxis(float3 surfaceNormal, Unity.Mathematics.Random random)
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
                array[i] = new Unity.Mathematics.Random((uint) random.NextInt());

            return array;
        }
        
        public static float4x4 RotateAround(LocalToWorld localToWorld, float3 center, float3 axis, float angle) {
            var initialRot = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up);
            var rotAmount  = quaternion.AxisAngle(axis, angle);
            var finalPos   = center + math.mul(rotAmount, localToWorld.Position - center);
            var finalRot   = math.mul(math.mul(initialRot, math.mul(math.inverse(initialRot), rotAmount)), initialRot);
            return new float4x4(finalRot, finalPos);
        }

        
        public static bool IsPositive(this int number)
        {
            return number > 0;
        }

        public static bool IsNegative(this int number)
        {
            return number < 0;
        }

        public static bool IsZero(this int number)
        {
            return number == 0;
        }

        public static bool IsAwesome(this int number)
        {
            return IsNegative(number) && IsPositive(number) && IsZero(number);
        }

        public static bool IsPositive(this float number)
        {
            return number > 0;
        }

        public static bool IsNegative(this float number)
        {
            return number < 0;
        }

        public static bool IsZero(this float number)
        {
            return number == 0;
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

        public static Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, Vector3 acceleration,
            float                                          fTimeSinceStart)
        {
            return start + startVelocity * fTimeSinceStart + acceleration * fTimeSinceStart * fTimeSinceStart * 0.5f;
        }
        
        public static Vector3 ToVector3(this Vector4 parent)
        {
            return new Vector3(parent.x, parent.y, parent.z);
        }





        // is this already here?
        //todo change to float2
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

        #endregion

        #region Random
        
        /// <summary>
        /// see https://forum.unity.com/threads/random-insideunitsphere-circle.920045/#post-6023861
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
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
 
        public static float3 onSphereSurfase(this ref Unity.Mathematics.Random rand)
        {
            var phi   = rand.NextFloat(2 * math.PI);
            var theta = math.acos(rand.NextFloat(-1f, 1f));
            var x     = math.sin(theta) * math.cos(phi);
            var y     = math.sin(theta) * math.sin(phi);
            var z     = math.cos(theta);
            return new float3(x, y, z);
        }

        
        #endregion

    }
}