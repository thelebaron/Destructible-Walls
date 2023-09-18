using System.Collections.Generic;
using Junk.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Core.Creation
{
     
/// <summary>
/// todo: On mesh import, if uvs do not match vertices length, error is thrown. fix uv out of range "error" though this may not be an actual error as in dcc programs uvs are correct
/// </summary>
    public static class ObjProcessor
    {
        struct Face
        {
            public int VertexCount;
            public FixedList32Bytes<int> VertexIndex;
            public FixedList32Bytes<int> TextureIndex;
            public FixedList32Bytes<int> NormalIndex;

            // subtract 1 for using in arrays(0 is start integer)
            public int A => VertexIndex[0]-1; 
            
            public int B => VertexIndex[1]-1;
            
            public int C => VertexIndex[2]-1;
            
            public int D =>  VertexIndex[3]-1;
            
            public FixedList32Bytes<int3> Triangles;


            public bool IsQuad()
            {
                return VertexCount.Equals(4);
            }
            public bool IsTri()
            {
                return VertexCount.Equals(3);
            }
            public bool IsNgon()
            {
                return VertexCount > 4 || VertexCount < 3;
            }
            
            
        }
        
        static int ToInt(this string input)
        {
            return System.Convert.ToInt32(input);
        }
        
        static         char[] componentIdentifier = {' '};
        static         char[] faceIdentifier      = {'/'};
        //private const string path                = "Packages/com.thelebaron.foundation/Foundation.Tests/DataAssets/cube.obj";
        //private static int    isQuad              = 4;
        //private static int    isTri               = 3;
        public static Mesh Read(string path)
        {
        
            //string text = System.IO.File.ReadAllText(path);
            string[] lines = System.IO.File.ReadAllLines(path);
            
            return GetMeshFromStringArray(lines);
        }
        
        public static Mesh GetMeshFromStringArray(string[] items)
        {
            var stringVerts = new List<string>();
            var stringUvs = new List<string>();
            var stringFaces = new List<string>();
            
            // split up deserialized string array into string component arrays
            for (var i = 0; i < items.Length; i++)
            {
                string item = items[i];
                
                // Skip comments
                if(/*item.Contains("//") || */item.Contains("#"))
                    continue;
                
                // Count all bits
                if (item.StartsWith("v "))
                {
                    stringVerts.Add(item);
                    continue;
                }
                
                                
                if (item.StartsWith("vt "))
                {
                    stringUvs.Add(item);
                    continue;
                }
                                
                if (item.StartsWith("f "))
                {
                    stringFaces.Add(item);
                    continue;
                }
            }
            
            Debug.Log("There are " + stringVerts.Count + " vertices, " + stringUvs.Count +" uvs and " + stringFaces.Count +" faces in the mesh.");
            
            // process vertices array
            var vertices = GetVertices(stringVerts);
            var uvs = GetUvs(stringUvs);
            var tris = GetTris(stringFaces, vertices);
            var mesh = new Mesh();
            
            mesh.vertices = vertices.ToArray().ToVector3();
            mesh.uv = uvs.ToArray().ToVector2();
            mesh.triangles = tris.ToArray();

            mesh.RecalculateNormals(60.0f, 0.5f);
            
            return mesh;
        }

        static List<float3> GetVertices(List<string> array)
        {
            var vertices = new List<float3>();
            for (int i = 0; i < array.Count; i++)
            {
                var line = array[i];
                line = line.Trim();
                var split = line.Split(componentIdentifier, 50);
                vertices.Add(new float3(System.Convert.ToSingle(split[1]), System.Convert.ToSingle(split[2]), System.Convert.ToSingle(split[3])));
            }

            return vertices;
        }
        
        static List<float2> GetUvs(List<string> array)
        {
            var uvs = new List<float2>();
            for (int i = 0; i < array.Count; i++)
            {
                var line = array[i];
                line = line.Trim();
                var split = line.Split(componentIdentifier, 50);
                uvs.Add(new float2(System.Convert.ToSingle(split[1]), System.Convert.ToSingle(split[2])));
            }

            return uvs;
        }

        /// <summary>
        /// Splits an obj face string into component parts 
        /// see https://en.wikipedia.org/wiki/Wavefront_.obj_file
        /// vertex_index/texture_index/normal_index
        ///    f 1/1 2/2 4/4 3/3/5
        /// </summary>
        private static Face GetFace(string line, List<float3> vertices)
        {
            var face = new Face();
            face.VertexIndex  = new FixedList32Bytes<int>();
            face.TextureIndex = new FixedList32Bytes<int>();
            face.NormalIndex  = new FixedList32Bytes<int>();
            face.Triangles = new FixedList32Bytes<int3>();
            // Remove face identifier
            line = line.Replace("f ", "");
            
            // split the line into parts
            var split = line.Split(componentIdentifier, 55);
            face.VertexCount = split.Length;
            
            // loop all indices
            // vertex_index/texture_index/normal_index
            //    f 1/1 2/2 4/4 3/3
            // 0) 1/1
            // 1) 2/2
            // 2) 4/4
            // 3) 3/3
            for (int j = 0; j < split.Length; j++)
            {
                // split the line into parts
                // vertex_index/texture_index/normal_index
                // operate on 1/1
                var vertUvNormal = split[j].Split(faceIdentifier, 3);
                // do these need to be initialized?
                //face.VertexIndex[j] = System.Convert.ToInt32(vertUvNormal[0]);
                face.VertexIndex.Add(System.Convert.ToInt32(vertUvNormal[0]));
                
                if (vertUvNormal.Length.Equals(1))
                    continue;
                
                //face.TextureIndex[j] = System.Convert.ToInt32(vertUvNormal[1]);
                face.TextureIndex.Add(System.Convert.ToInt32(vertUvNormal[1]));
                
                if (vertUvNormal.Length.Equals(2))
                    continue;
                
                //face.NormalIndex[j] = System.Convert.ToInt32(vertUvNormal[2]);
                face.NormalIndex.Add(System.Convert.ToInt32(vertUvNormal[2]));
            }
    
            // http://james-ramsden.com/triangulate-a-quad-mesh-in-c/
            if (face.IsQuad())
            {
                var d1 = math.distance(vertices[face.A], vertices[face.C]);
                var d2 = math.distance(vertices[face.B], vertices[face.D]);
                if (d1 > d2)
                {
                    //x.Faces.AddFace(mf.A, mf.B, mf.D);
                    //x.Faces.AddFace(mf.B, mf.C, mf.D);
                    face.Triangles.Add(new int3(face.A,face.B,face.D));
                    face.Triangles.Add(new int3(face.B,face.C,face.D));
                }
                else
                {
                    //x.Faces.AddFace(mf.A, mf.B, mf.C);
                    //x.Faces.AddFace(mf.A, mf.C, mf.D);
                    face.Triangles.Add(new int3(face.A,face.B,face.C));
                    face.Triangles.Add(new int3(face.A,face.C,face.D));
                }
            }
            if (face.IsTri())
            {
                face.Triangles.Add(new int3(face.A,face.B,face.C));
            }
            return face;
        }
        
        static List<int> GetTris(List<string> faceArray, List<float3> vertices)
        {
            var tris = new List<int>();

            for (int i = 0; i < faceArray.Count; i++)
            {
                var line = faceArray[i];
                
                // Remove face identifier
                line = line.Replace("f ", "");
                
                // split the line into parts
                var faceSplitString = line.Split(componentIdentifier, 55);
                
                var face = GetFace(line, vertices);
                for (int j = 0; j < face.Triangles.Length; j++)
                {
                    tris.Add(face.Triangles[j].x);
                    tris.Add(face.Triangles[j].y);
                    tris.Add(face.Triangles[j].z);
                }
            }
            
            return tris;
        }


    }
}