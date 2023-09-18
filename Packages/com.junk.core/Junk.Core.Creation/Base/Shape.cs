using System;
using System.Collections.Generic;
using Junk.Math;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
// ReSharper disable InconsistentNaming

namespace Junk.Core.Creation
{
    /// <summary>
    /// Data class to serialize a unityengine mesh to save friendly format.
    /// See https://docs.unity3d.com/ScriptReference/Mesh.html
    /// </summary>
    [Serializable]
    public struct Shape
    {
        [SerializeField] public float4x4[]   bindPoses;
        [SerializeField] public BoneWeight[] boneWeights;
        [SerializeField] public ShapeBounds  shapeBounds;
        [SerializeField] public colour[]     colors;
        [SerializeField] public IndexFormat  indexFormat;
        [SerializeField] public float3[]     vertices;
        [SerializeField] public int[]        triangles;
        [SerializeField] public float2[]     uv1;
        [SerializeField] public float2[]     uv2;
        [SerializeField] public float2[]     uv3;
        [SerializeField] public float2[]     uv4;
        [SerializeField] public float3[]     normals;

        
        #region Conversion

        /// <summary>
        ///  Constructor: takes a mesh and fills out SerializableMeshInfo data structure which basically mirrors Mesh object's parts.
        /// </summary>
        /// <param name="mesh">UnityEngine mesh to serialize</param>
        public Shape(Mesh mesh)
        {
            shapeBounds = new ShapeBounds
            {
                Center  = mesh.bounds.center,
                Extents = mesh.bounds.extents
            };
            indexFormat = mesh.indexFormat;

            // todo are submeshes needed?
            //SubMeshDescriptor sub = mesh.GetSubMesh(9);

            bindPoses   = new float4x4[mesh.bindposes.Length];
            boneWeights = new BoneWeight[mesh.boneWeights.Length];
            vertices    = new float3[mesh.vertexCount];
            triangles   = new int[mesh.triangles.Length]; // initialize triangles array
            uv1         = new float2[mesh.uv.Length];
            uv2 = new float2[mesh.uv2.Length];
            uv3         = new float2[mesh.uv3.Length];
            uv4         = new float2[mesh.uv4.Length];
            normals     = new float3[mesh.normals.Length];
            colors      = new colour[mesh.colors.Length];

            for (int i = 0; i < mesh.bindposes.Length; i++)
                bindPoses[i] = mesh.bindposes[i];

            for (int i = 0; i < mesh.boneWeights.Length; i++)
                boneWeights[i] = mesh.boneWeights[i];

            for (int i = 0; i < mesh.vertexCount; i++)
                vertices[i] = mesh.vertices[i];

            for (int i = 0; i < mesh.triangles.Length; i++) // Mesh's triangles is an array that stores the indices, sequentially, of the vertices that form one face
                triangles[i] = mesh.triangles[i];

            // Get uvs
            for (int i = 0; i < mesh.uv.Length; i++)
                uv1[i] = mesh.uv[i];
            for (int i = 0; i < mesh.uv2.Length; i++)
                uv2[i] = mesh.uv2[i];
            for (int i = 0; i < mesh.uv3.Length; i++)
                uv3[i] = mesh.uv3[i];
            for (int i = 0; i < mesh.uv4.Length; i++)
                uv4[i] = mesh.uv4[i];

            for (int i = 0; i < mesh.normals.Length; i++)
                normals[i] = mesh.normals[i];

            for (int i = 0; i < mesh.colors.Length; i++)
                colors[i] = new colour(mesh.colors[i]);
        }

        /// <summary>
        /// GetMesh gets a Mesh object from currently set data in this SerializableMeshData object
        /// Sequential values are deserialized to Mesh original data types like Vector3 for vertices.
        /// </summary>
        /// <returns></returns>
        public Mesh GetMesh()
        {
            var mesh = new Mesh();

            mesh.bounds = new Bounds
            {
                center  = shapeBounds.Center,
                extents = shapeBounds.Extents
            };
            mesh.indexFormat = indexFormat;
            //mesh.sub

            var bindPoseList   = new List<Matrix4x4>();
            var boneweightList = new List<BoneWeight>();
            var verticesList   = new List<Vector3>();
            var uvList         = new List<Vector2>();
            var uv2List        = new List<Vector2>();
            var uv3List        = new List<Vector2>();
            var uv4List        = new List<Vector2>();
            var normalsList    = new List<Vector3>();

            for (int i = 0; i < bindPoses.Length; i++)
                bindPoseList.Add(bindPoses[i]);
            mesh.bindposes = bindPoseList.ToArray();

            for (int i = 0; i < boneWeights.Length; i++)
                boneweightList.Add(boneWeights[i]);
            mesh.boneWeights = boneweightList.ToArray();

            for (int i = 0; i < vertices.Length; i++)
                verticesList.Add(vertices[i]);

            mesh.SetVertices(verticesList);

            mesh.triangles = triangles;

            for (var i = 0; i < uv1.Length; i++)
                uvList.Add(uv1[i]);
            mesh.SetUVs(0, uvList);

            for (var i = 0; i < uv2.Length; i++)
                uv2List.Add(uv2[i]);
            mesh.SetUVs(1, uv2List);

            for (var i = 0; i < uv3.Length; i++)
                uv2List.Add(uv3[i]);
            mesh.SetUVs(2, uv3List);

            for (var i = 0; i < uv4.Length; i++)
                uv2List.Add(uv4[i]);
            mesh.SetUVs(3, uv4List);

            for (var i = 0; i < normals.Length; i++)
                normalsList.Add(normals[i]);
            mesh.SetNormals(normalsList);

            mesh.boneWeights = boneWeights;
            mesh.colors      = colour.ToArray(colors);

            return mesh;
        }

        public static void WriteNativeArray<T>(Unity.Entities.Serialization.BinaryWriter writer, NativeArray<T> array) where T : struct
        {
            int lengthBytes = (int) array.Length * (int) UnsafeUtility.SizeOf<T>();
            // we don't need to write both, but it's just for sanity checks/debugging (and in case the size of the type changes)
            writer.Write((int) array.Length);
            writer.Write(lengthBytes);
            writer.WriteArray(array);
        }
        
        public static NativeArray<T> ReadNativeArray<T>(Unity.Entities.Serialization.BinaryReader reader) where T : struct
        {
            var length      = reader.ReadInt();
            var lengthBytes = length * (int) UnsafeUtility.SizeOf<T>();
            var checkBytes  = reader.ReadInt();
            if (lengthBytes != checkBytes)
                throw new Exception($"Corrupted/invalid data -- expected to read constant {lengthBytes}, but actually read {checkBytes}");

            var array = new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var fail  = true;
            try
            {
                reader.ReadArray(array, length);
                fail = false;
                return array;
            }
            finally
            {
                if (fail) array.Dispose();
            }
        }
        
        #endregion
        
        #region Types

        public struct ShapeBounds
        {
            public float3 Center;
            public float3 Extents;

            public Bounds ToBounds()
            {
                return new Bounds
                {
                    center = Center, extents = Extents
                };
            }
        

        }
        

        #endregion
    }
}