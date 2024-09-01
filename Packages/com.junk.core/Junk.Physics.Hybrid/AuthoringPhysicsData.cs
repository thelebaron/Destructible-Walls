using System;
using Junk.Math;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.Serialization;

namespace Junk.Physics.Hybrid
{
    /// <summary>
    /// Just an authoring class to assist with making the fields look a little tidier.
    /// </summary>
    [Serializable]
    public struct AuthoringPhysicsData
    {
        public enum GeoType
        {
            Box,
            Sphere,
            Capsule,
            ConvexHull,
            Mesh
        }

        public float3          Center;
        public float3          Extents;
        public float           Mass;
        public GeoType         Geometry;
        public CollisionFilter Filter;

        public AuthoringPhysicsData(float3 extents, float3 center = default,  float mass = 1, GeoType geometry = GeoType.Box)
        {
            Extents  = extents;
            Center   = center;
            Mass     = mass;
            Geometry = geometry;
            Filter   = CollisionFilter.Default;
        }
        
    }
}