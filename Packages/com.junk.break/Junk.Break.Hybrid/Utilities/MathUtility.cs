using Unity.Mathematics;
using UnityEngine;

namespace Junk.Break.Hybrid
{
    public static class MathUtility
    {
        
        public static bool SameVector(Vector3 lhs, Vector3 rhs)
        {
            
            var x   = System.Math.Round(lhs.x, 2);
            var y   = System.Math.Round(lhs.y, 2);
            var z   = System.Math.Round(lhs.z, 2);
            var xyz = new double3(x,y,z);
            
            var a   = System.Math.Round(rhs.x, 2);
            var b   = System.Math.Round(rhs.y, 2);
            var c   = System.Math.Round(rhs.z, 2);
            var abc = new double3(a,b,c);

            return xyz.Equals(abc);

        }
    }
}