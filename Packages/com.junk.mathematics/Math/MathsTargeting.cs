using Unity.Mathematics;

namespace Junk.Math
{
    public static partial class maths
    {
        public static bool facing(quaternion selfRotation, float3 self, float3 other)
        {
            var normalized = math.normalize(other - self);
            var dot        = math.dot(normalized, math.forward(selfRotation));
            if (dot > 0.99f)
                return true;
            return false;
        }
        
        public static bool facing(float4x4 self, float4x4 other)
        {
            var selfPosition  = new float3(self.c3.x, self.c3.y, self.c3.z);
            var selfRotation  = new quaternion(math.orthonormalize(new float3x3(self)));
            var otherPosition = new float3(other.c3.x, other.c3.y, other.c3.z);
            
            var normalized = math.normalize(otherPosition - selfPosition);
            var dot        = math.dot(normalized, math.forward(selfRotation));
            if (dot > 0.99f)
                return true;
            return false;
        }

        public static bool infront(quaternion selfRotation, float3 self, float3 other)
        {
            var normalized = math.normalize(other - self);
            var dot        = math.dot(normalized, math.forward(selfRotation));
            if (dot > 0.3f)
                return true;
            return false;
        }
        
        public static bool insidePoint(float3 a, float3 b, float radius)
        {
            var distance = math.distance(a, b);
            if (distance < radius)
                return true;
            return false;
        }
    }
}