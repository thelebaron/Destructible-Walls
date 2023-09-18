using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Junk.Entities
{
    /// <summary>
    /// Keep in sync with ColliderCastHit
    /// </summary>
    [Serializable]
    public struct Trace
    {
        public float Fraction;
        public int RigidBodyIndex;
        public ColliderKey ColliderKey;
        public Material Material;
        public Entity Entity;
        public float3 Position;
        public float Distance;
        public float3 SurfaceNormal;

        public static implicit operator ColliderCastHit(Trace trace)
        {
            return new ColliderCastHit
            {
                ColliderKey    = trace.ColliderKey,
                Entity         = trace.Entity,
                Fraction       = trace.Fraction,
                Material       = trace.Material,
                Position       = trace.Position,
                RigidBodyIndex = trace.RigidBodyIndex,
                SurfaceNormal  = trace.SurfaceNormal
            };
        }

        public static implicit operator Trace(ColliderCastHit cast)
        {
            return new Trace
            {
                ColliderKey    = cast.ColliderKey,
                Entity         = cast.Entity,
                Fraction       = cast.Fraction,
                Material       = cast.Material,
                Position       = cast.Position,
                RigidBodyIndex = cast.RigidBodyIndex,
                SurfaceNormal  = cast.SurfaceNormal
            };
        }
    }
}