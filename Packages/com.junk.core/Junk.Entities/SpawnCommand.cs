using Unity.Mathematics;

namespace Junk.Entities
{
    public struct SpawnCommand
    {
        public VFXType Type;
        public float3  Position;
        public float3  Normal;
        public float3  Angle; // apears to be world space euler angle
        public float3  Velocity;
        public float   Size;
        public int     SpawnCount;

        public enum VFXType
        {
            Smoke,
            Spark,
            BloodSpray1,
            BloodMist
        }
    }
}