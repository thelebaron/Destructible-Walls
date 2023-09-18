using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.Core.Creation
{
    /// <summary>
    /// Data class to serialize a unityengine mesh to save friendly format.
    /// See https://docs.unity3d.com/ScriptReference/Mesh.html
    /// </summary>
    [Serializable]
    public struct BlobMesh
    {
        public BlobString            name;
        public BlobArray<float4x4>   bindPoses;
        public BlobArray<BoneWeight> boneWeights;
        public Bounds                bounds;
        public BlobArray<colour>     colors;
        public IndexFormat           indexFormat;
        public BlobArray<float3>     vertices;
        public BlobArray<int>        triangles;
        public BlobArray<float2>     uv;
        public BlobArray<float2>     uv2;
        public BlobArray<float2>     uv3;
        public BlobArray<float2>     uv4;
        public BlobArray<float2>     uv5;
        public BlobArray<float2>     uv6;
        public BlobArray<float2>     uv7;
        public BlobArray<float2>     uv8;
        public BlobArray<float3>     normals;
        public BlobArray<float4>     tangents;
        public BlobArray<SubMesh>    submeshes;

        
        /// <summary>
        /// Must be manually disposed
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static unsafe BlobBuilder ConstructBlobBuilder(Mesh mesh)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobMesh>();

            builder.AllocateString(ref root.name, mesh.name);
            
            root.bounds      = mesh.bounds;
            root.indexFormat = mesh.indexFormat;

            var bindPoseArray = builder.Allocate<float4x4>(ref root.bindPoses, mesh.bindposes.Length);
            for (int i = 0; i < bindPoseArray.Length; i++)
                bindPoseArray[i] = mesh.bindposes[i];
            var boneweightArray = builder.Allocate<BoneWeight>(ref root.boneWeights, mesh.boneWeights.Length);
            for (int i = 0; i < boneweightArray.Length; i++)
                boneweightArray[i] = mesh.boneWeights[i];

            var colorsArray = builder.Allocate(ref root.colors, mesh.colors.Length);
            for (int i = 0; i < colorsArray.Length; i++)
                colorsArray[i] = mesh.colors[i];

            var verts = builder.Allocate(ref root.vertices, mesh.vertices.Length);
            for (int i = 0; i < verts.Length; i++)
                verts[i] = mesh.vertices[i];
            var tris = builder.Allocate(ref root.triangles, mesh.triangles.Length);
            for (int i = 0; i < tris.Length; i++)
                tris[i] = mesh.triangles[i];

            var uv = builder.Allocate(ref root.uv, mesh.uv.Length);
            for (int i = 0; i < uv.Length; i++)
                uv[i] = mesh.uv[i];
            var uv2 = builder.Allocate(ref root.uv2, mesh.uv2.Length);
            for (int i = 0; i < uv2.Length; i++)
                uv2[i] = mesh.uv2[i];
            var uv3 = builder.Allocate(ref root.uv3, mesh.uv3.Length);
            for (int i = 0; i < uv3.Length; i++)
                uv3[i] = mesh.uv3[i];
            var uv4 = builder.Allocate(ref root.uv4, mesh.uv4.Length);
            for (int i = 0; i < uv4.Length; i++)
                uv4[i] = mesh.uv4[i];
            var uv5 = builder.Allocate(ref root.uv5, mesh.uv5.Length);
            for (int i = 0; i < uv5.Length; i++)
                uv5[i] = mesh.uv5[i];
            var uv6 = builder.Allocate(ref root.uv6, mesh.uv6.Length);
            for (int i = 0; i < uv6.Length; i++)
                uv6[i] = mesh.uv6[i];
            var uv7 = builder.Allocate(ref root.uv7, mesh.uv7.Length);
            for (int i = 0; i < uv7.Length; i++)
                uv7[i] = mesh.uv7[i];
            var uv8 = builder.Allocate(ref root.uv8, mesh.uv8.Length);
            for (int i = 0; i < uv8.Length; i++)
                uv8[i] = mesh.uv8[i];

            var normals = builder.Allocate(ref root.normals, mesh.normals.Length);
            for (int i = 0; i < normals.Length; i++)
                normals[i] = mesh.normals[i];
            var tangents = builder.Allocate(ref root.tangents, mesh.tangents.Length);
            for (int i = 0; i < tangents.Length; i++)
                tangents[i] = mesh.tangents[i];

            var submeshes = builder.Allocate(ref root.submeshes, mesh.subMeshCount);
            for (int i = 0; i < submeshes.Length; i++)
                submeshes[i] = mesh.GetSubMesh(i);
            
            return builder;
        }

        public static BlobAssetReference<BlobMesh> ToBlobData(Mesh mesh)
        {
            var     builder = new BlobBuilder(Allocator.Temp);
            ref var root     = ref builder.ConstructRoot<BlobMesh>();

            builder.AllocateString(ref root.name, mesh.name);
            
            root.bounds      = mesh.bounds;
            root.indexFormat = mesh.indexFormat;

            var bindPoseArray = builder.Allocate<float4x4>(ref root.bindPoses, mesh.bindposes.Length);
            for (int i = 0; i < bindPoseArray.Length; i++)
                bindPoseArray[i] = mesh.bindposes[i];
            
            var boneweightArray = builder.Allocate<BoneWeight>(ref root.boneWeights, mesh.boneWeights.Length);
            for (int i = 0; i < boneweightArray.Length; i++)
                boneweightArray[i] = mesh.boneWeights[i];
            var colorsArray = builder.Allocate(ref root.colors, mesh.colors.Length);
            for (int i = 0; i < colorsArray.Length; i++)
                colorsArray[i] = mesh.colors[i];

            var verts = builder.Allocate(ref root.vertices, mesh.vertices.Length);
            for (int i = 0; i < verts.Length; i++)
                verts[i] = mesh.vertices[i];
            var tris = builder.Allocate(ref root.triangles, mesh.triangles.Length);
            for (int i = 0; i < tris.Length; i++)
                tris[i] = mesh.triangles[i];

            var uv = builder.Allocate(ref root.uv, mesh.uv.Length);
            for (int i = 0; i < uv.Length; i++)
                uv[i] = mesh.uv[i];
            var uv2 = builder.Allocate(ref root.uv2, mesh.uv2.Length);
            for (int i = 0; i < uv2.Length; i++)
                uv2[i] = mesh.uv2[i];
            var uv3 = builder.Allocate(ref root.uv3, mesh.uv3.Length);
            for (int i = 0; i < uv3.Length; i++)
                uv3[i] = mesh.uv3[i];
            var uv4 = builder.Allocate(ref root.uv4, mesh.uv4.Length);
            for (int i = 0; i < uv4.Length; i++)
                uv4[i] = mesh.uv4[i];
            var uv5 = builder.Allocate(ref root.uv5, mesh.uv5.Length);
            for (int i = 0; i < uv5.Length; i++)
                uv5[i] = mesh.uv5[i];
            var uv6 = builder.Allocate(ref root.uv6, mesh.uv6.Length);
            for (int i = 0; i < uv6.Length; i++)
                uv6[i] = mesh.uv6[i];
            var uv7 = builder.Allocate(ref root.uv7, mesh.uv7.Length);
            for (int i = 0; i < uv7.Length; i++)
                uv7[i] = mesh.uv7[i];
            var uv8 = builder.Allocate(ref root.uv8, mesh.uv8.Length);
            for (int i = 0; i < uv8.Length; i++)
                uv8[i] = mesh.uv8[i];

            var normals = builder.Allocate(ref root.normals, mesh.normals.Length);
            for (int i = 0; i < normals.Length; i++)
                normals[i] = mesh.normals[i];
            var tangents = builder.Allocate(ref root.tangents, mesh.tangents.Length);
            for (int i = 0; i < tangents.Length; i++)
                tangents[i] = mesh.tangents[i];

            var submeshes = builder.Allocate(ref root.submeshes, mesh.subMeshCount);
            for (int i = 0; i < submeshes.Length; i++)
                submeshes[i] = mesh.GetSubMesh(i);


            var blob = builder.CreateBlobAssetReference<BlobMesh>(Allocator.Persistent);
            root = ref blob.Value;
            
            builder.Dispose();

            return blob;
        }

        public Mesh ToMesh()
        {
            var mesh = new Mesh();

            mesh.name = name.ToString();
            
            mesh.bindposes = bindPoses.AsType<Matrix4x4,float4x4>(); // does this work????
            mesh.colors = colors.AsType<Color,colour>();
            mesh.vertices = vertices.AsType<Vector3, float3>();
            mesh.uv = uv.AsType<Vector2, float2>();
            mesh.uv2 = uv2.AsType<Vector2, float2>();
            mesh.uv3 = uv3.AsType<Vector2, float2>();
            mesh.uv4 = uv4.AsType<Vector2, float2>();
            mesh.uv5 = uv5.AsType<Vector2, float2>();
            mesh.uv6 = uv6.AsType<Vector2, float2>();
            mesh.uv7 = uv7.AsType<Vector2, float2>();
            mesh.uv8 = uv8.AsType<Vector2, float2>();
            mesh.normals = normals.AsType<Vector3, float3>();
            mesh.tangents = tangents.AsType<Vector4, float4>();
            
            //mesh.bindposes = bindPoses.ToArray().ToMatrix4x4();
            mesh.boneWeights = boneWeights.ToArray();
            mesh.bounds = bounds;
            //mesh.colors = colors.ToArray().ToColor();
            //mesh.vertices = vertices.ToArray().ToVector3();
            mesh.triangles = triangles.ToArray();
            /*mesh.uv = uv.ToArray().ToVector2();
            mesh.uv2 = uv2.ToArray().ToVector2();
            mesh.uv3 = uv3.ToArray().ToVector2();
            mesh.uv4 = uv4.ToArray().ToVector2();
            mesh.uv5 = uv5.ToArray().ToVector2();
            mesh.uv6 = uv6.ToArray().ToVector2();
            mesh.uv7 = uv7.ToArray().ToVector2();
            mesh.uv8 = uv8.ToArray().ToVector2();
            mesh.normals = normals.ToArray().ToVector3();
            mesh.tangents = tangents.ToArray().ToVector4();*/
            
            //mesh.sub
            return mesh;
        }

        

        public struct Bounds
        {
            public float3 Center;
            public float3 Extents;

            public static implicit operator UnityEngine.Bounds(Bounds bounds)
            {
                return new UnityEngine.Bounds
                {
                    center = bounds.Center, extents = bounds.Extents
                };
            }

            public static implicit operator Bounds(UnityEngine.Bounds bounds)
            {
                return new Bounds
                {
                    Center = bounds.center, Extents = bounds.extents
                };
            }
        }
    }
}