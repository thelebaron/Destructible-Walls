using System;
using Junk.Math;
using Unity.Mathematics;
using Unity.Physics;

namespace Junk.Physics.Hybrid
{
    /// <summary>
    /// Just an authoring class to assist with making the fields look a little tidier.
    /// </summary>
    [Serializable]
    public class AuthoringPhysicsData
    {
        public enum PhysicsAuthoringColliderType
        {
            Box,
            Sphere,
            Capsule,
            ConvexHull,
            Mesh
        }

        public float3                       Size;
        public float3                       Offset;
        public float                        Mass     = 1;
        public PhysicsAuthoringColliderType Geometry = PhysicsAuthoringColliderType.Box;

        public AuthoringPhysicsData(float3 size, float3 offset = default,  float mass = 1, PhysicsAuthoringColliderType geometry = PhysicsAuthoringColliderType.Box)
        {
            Size   = size;
            Offset = offset;
            Mass   = mass;
            Geometry = geometry;
        }
        
    }
}