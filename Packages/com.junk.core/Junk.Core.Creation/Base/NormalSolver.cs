/**
 * The following code is derived from the original code from: http://schemingdeveloper.com.
 * The derived version was developed by Rune Skovbo Johansen - http://runevision.com
 * 
 * Modifications in the derived version:
 * 
 *  - Averaged normals are calculated as a weighted average based on face area,
 *    known as "face weighted normals" or "area weighted normals".
 * 
 *  - An ignoreFactor parameter has been added which can cull normals from the average
 *    if their weight is smaller than a certain fraction of the largest weight.
 *    This can be used to make large faces completely flat, distributing the curvature
 *    completely to adjacent smaller faces.
 */

/**
 * The following code was taken from: http://schemingdeveloper.com
 *
 * Visit our game studio website: http://stopthegnomes.com
 *
 * License: You may use this code however you see fit, as long as you give credit when
 * 			explicitly asked and as long as you include this notice without any modifications.
 *
 * 			You may not publish a paid asset on Unity store if its main function is based on
 *			the following code, but you may publish a paid asset that uses this as part of a
 *			larger suite. You may still publish a free asset whose main function is using this
 *			script if you give us credit in the asset description.
 *
 *			If you intend to use this in a Unity store asset, it would be appreciated, but
 *			not required, if you let us know with a link to the asset.
 */

using System;
using System.Collections.Generic;
using UnityEngine;


namespace Junk.Core.Creation
{
    public static class NormalSolver
    {
        /// <summary>
        ///     Recalculate the normals of a mesh based on an angle threshold. This takes
        ///     into account distinct vertices that have the same position.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="angle">
        ///     The smoothing angle. Note that triangles that already share
        ///     the same vertex will be smooth regardless of the angle!
        /// </param>
        /// <param name="ignoreFactor">
        ///     A fraction between 0 and 1.
        ///     Weights smaller than this fraction relative to the largest weight are ignored.
        /// </param>
        public static void RecalculateNormals(this Mesh mesh, float angle, float ignoreFactor)
        {
            var triangles          = mesh.GetTriangles(0);
            var vertices           = mesh.vertices;
            var triNormals         = new Vector3[triangles.Length / 3]; //Normal of each triangle
            var triNormalsWeighted = new Vector3[triangles.Length / 3]; //Weighted normal of each triangle
            var normals            = new Vector3[vertices.Length];

            angle = angle * Mathf.Deg2Rad;

            var dictionary = new Dictionary<VertexKey, VertexEntry>(vertices.Length);

            //Goes through all the triangles and gathers up data to be used later
            for (var i = 0; i < triangles.Length; i += 3)
            {
                int i1 = triangles[i];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                //Calculate the normal of the triangle
                Vector3 p1 = vertices[i2] - vertices[i1];
                Vector3 p2 = vertices[i3] - vertices[i1];

                // By not normalizing the cross product,
                // the face area is pre-multiplied onto the normal for free.
                Vector3 normal   = Vector3.Cross(p1, p2);
                int     triIndex = i / 3;
                triNormalsWeighted[triIndex] = normal;
                triNormals[triIndex]         = normal.normalized;

                VertexEntry entry;
                VertexKey   key;

                //For each of the three points of the triangle
                //  > Add this triangle as part of the triangles they're connected to.

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out entry))
                {
                    entry = new VertexEntry();
                    dictionary.Add(key, entry);
                }

                entry.Add(i1, triIndex);

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry))
                {
                    entry = new VertexEntry();
                    dictionary.Add(key, entry);
                }

                entry.Add(i2, triIndex);

                if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry))
                {
                    entry = new VertexEntry();
                    dictionary.Add(key, entry);
                }

                entry.Add(i3, triIndex);
            }

            //Foreach point in space (not necessarily the same vertex index!)
            //{
            //  Foreach triangle T1 that point belongs to
            //  {
            //    Foreach other triangle T2 (including self) that point belongs to and that
            //    meets any of the following:
            //    1) The corresponding vertex is actually the same vertex
            //    2) The angle between the two triangles is less than the smoothing angle
            //    {
            //      > Add to the set of contributing normals
            //    }
            //    > Add the normals in the set together, excluding those smaller than the threshold.
            //    > Normalize the resulting vector to find the average
            //    > Assign the normal to corresponding vertex of T1
            //  }
            //}

            List<Vector3> normalSet = new List<Vector3>();
            foreach (var value in dictionary.Values)
            {
                for (var i = 0; i < value.Count; ++i)
                {
                    normalSet.Clear();
                    float longest = 0;
                    for (var j = 0; j < value.Count; ++j)
                    {
                        bool use = false;
                        if (value.VertexIndex[i] == value.VertexIndex[j])
                        {
                            use = true;
                        }
                        else
                        {
                            float dot = Vector3.Dot(
                                triNormals[value.TriangleIndex[i]],
                                triNormals[value.TriangleIndex[j]]);
                            dot = Mathf.Clamp(dot, -0.99999f, 0.99999f);
                            float acos = Mathf.Acos(dot);
                            if (acos <= angle)
                                use = true;
                        }

                        if (use)
                        {
                            Vector3 normal = triNormalsWeighted[value.TriangleIndex[j]];
                            normalSet.Add(normal);
                            float length = normal.magnitude;
                            if (length > longest)
                                longest = length;
                        }
                    }

                    var sum       = new Vector3();
                    var threshold = longest * ignoreFactor;
                    for (int j = 0; j < normalSet.Count; j++)
                    {
                        if (normalSet[j].magnitude >= threshold)
                            sum += normalSet[j];
                    }

                    normals[value.VertexIndex[i]] = sum.normalized;
                }
            }

            mesh.normals = normals;
        }

        private struct VertexKey
        {
            private readonly long _x;
            private readonly long _y;
            private readonly long _z;

            //Change this if you require a different precision.
            private const int Tolerance = 100000;

            public VertexKey(Vector3 position)
            {
                _x = (long) (Mathf.Round(position.x * Tolerance));
                _y = (long) (Mathf.Round(position.y * Tolerance));
                _z = (long) (Mathf.Round(position.z * Tolerance));
            }

            public override bool Equals(object obj)
            {
                var key = (VertexKey) obj;
                return _x == key._x && _y == key._y && _z == key._z;
            }

            public override int GetHashCode()
            {
                return (_x * 7 ^ _y * 13 ^ _z * 27).GetHashCode();
            }
        }

        private sealed class VertexEntry
        {
            public int[] TriangleIndex = new int[4];
            public int[] VertexIndex   = new int[4];

            private int _reserved = 4;
            private int _count;

            public int Count
            {
                get { return _count; }
            }

            public void Add(int vertIndex, int triIndex)
            {
                //Auto-resize the arrays when needed
                if (_reserved == _count)
                {
                    _reserved *= 2;
                    Array.Resize(ref TriangleIndex, _reserved);
                    Array.Resize(ref VertexIndex, _reserved);
                }

                TriangleIndex[_count] = triIndex;
                VertexIndex[_count]   = vertIndex;
                ++_count;
            }
        }
    }
}