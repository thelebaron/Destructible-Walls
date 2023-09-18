using System.Collections;
using System.IO;
using NUnit.Framework;
using Junk.Core.Creation;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Junk.Core.Tests
{
    public class BlobMeshGeometry
    {
        private GameObject primitive;
        private Mesh referenceMesh;
        private BlobAssetReference<BlobMesh> meshBlobData;

        [SetUp]
        public void Setup()
        {
            primitive = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var filter  = primitive.GetComponent<MeshFilter>();
            
            referenceMesh = filter.sharedMesh;
            meshBlobData = BlobMesh.ToBlobData(referenceMesh);
            
        }

        [TearDown]
        protected void TearDown()
        {
            Object.DestroyImmediate(primitive);
            meshBlobData.Dispose();
        }
        
        
        [Test]
        public void BlobMeshGeometryCreation()
        {
            ValidateBlobData(ref meshBlobData.Value, referenceMesh);
        }
        
        static void ValidateBlobData(ref BlobMesh root, Mesh mesh)
        {
            Assert.AreEqual(root.name.Length, mesh.name.Length);
            Assert.AreEqual(root.name.ToString(), mesh.name);
            
            Assert.AreEqual(root.bindPoses.Length, mesh.bindposes.Length);
            for (int i = 0; i < root.bindPoses.Length; i++)
                Assert.AreEqual(mesh.bindposes[i], (Matrix4x4)root.bindPoses[i]);
            
            Assert.AreEqual(root.boneWeights.Length, mesh.boneWeights.Length);
            for (int i = 0; i < root.boneWeights.Length; i++)
                Assert.AreEqual(mesh.boneWeights[i], root.boneWeights[i]);
            
            Assert.AreEqual(mesh.bounds.center, (Vector3)root.bounds.Center);
            Assert.AreEqual(mesh.bounds.extents, (Vector3)root.bounds.Extents);
            
            Assert.AreEqual(root.colors.Length, mesh.colors.Length);
            for (int i = 0; i < root.colors.Length; i++)
                Assert.AreEqual(mesh.colors[i], (Color)root.colors[i]);

            Assert.AreEqual(mesh.indexFormat, root.indexFormat);
            
            Assert.AreEqual(root.vertices.Length, mesh.vertices.Length);
            for (int i = 0; i < root.vertices.Length; i++)
                Assert.AreEqual( mesh.vertices[i], (Vector3)root.vertices[i]);
            
                        
            Assert.AreEqual(root.uv.Length, mesh.uv.Length);
            for (int i = 0; i < root.uv.Length; i++)
                Assert.AreEqual( mesh.uv[i], (Vector2)root.uv[i]);

            Assert.AreEqual(root.uv2.Length, mesh.uv2.Length);
            for (int i = 0; i < root.uv2.Length; i++)
                Assert.AreEqual( mesh.uv2[i], (Vector2)root.uv2[i]);
            Assert.AreEqual(root.uv3.Length, mesh.uv3.Length);
            for (int i = 0; i < root.uv3.Length; i++)
                Assert.AreEqual( mesh.uv3[i], (Vector2)root.uv3[i]);
            Assert.AreEqual(root.uv4.Length, mesh.uv4.Length);
            for (int i = 0; i < root.uv4.Length; i++)
                Assert.AreEqual( mesh.uv4[i], (Vector2)root.uv4[i]);
            Assert.AreEqual(root.uv5.Length, mesh.uv5.Length);
            for (int i = 0; i < root.uv5.Length; i++)
                Assert.AreEqual( mesh.uv5[i], (Vector2)root.uv5[i]);
            Assert.AreEqual(root.uv6.Length, mesh.uv6.Length);
            for (int i = 0; i < root.uv6.Length; i++)
                Assert.AreEqual( mesh.uv6[i], (Vector2)root.uv6[i]);
            Assert.AreEqual(root.uv7.Length, mesh.uv7.Length);
            for (int i = 0; i < root.uv7.Length; i++)
                Assert.AreEqual( mesh.uv7[i], (Vector2)root.uv7[i]);
            Assert.AreEqual(root.uv8.Length, mesh.uv8.Length);
            for (int i = 0; i < root.uv8.Length; i++)
                Assert.AreEqual( mesh.uv8[i], (Vector2)root.uv8[i]);
            
            
            Assert.AreEqual(root.triangles.Length, mesh.triangles.Length);
            for (int i = 0; i < root.triangles.Length; i++)
                Assert.AreEqual( mesh.triangles[i], root.triangles[i]);
            
            Assert.AreEqual(root.normals.Length, mesh.normals.Length);
            for (int i = 0; i < root.normals.Length; i++)
                Assert.AreEqual(mesh.normals[i], (Vector3)root.normals[i]);
            
            Assert.AreEqual(root.tangents.Length, mesh.tangents.Length);
            for (int i = 0; i < root.tangents.Length; i++)
                Assert.AreEqual(mesh.tangents[i], (Vector4)root.tangents[i]);

            
            Assert.AreEqual(root.submeshes.Length, mesh.subMeshCount);
            for (int i = 0; i < root.submeshes.Length; i++)
                Assert.AreEqual(mesh.GetSubMesh(i), (SubMeshDescriptor)root.submeshes[i]);

            
        }

        const int kVersion          = 51;
        const int kIncorrectVersion = 13;
        
 
        [Test]
        public void BlobAssetBuilder()
        {
            string path = "Packages/com.junk.core/Junk.Core.Creation.Tests/DataAssets/BlobAssetMeshGeometry.blob";
            if (File.Exists(path))
                File.Delete(path);

            try
            {
                var builder = BlobMesh.ConstructBlobBuilder(referenceMesh);
                BlobAssetReference<BlobMesh>.Write(builder, path, kVersion);

                /*using (var reader = new StreamBinaryReader(path))
                    Assert.IsFalse(BlobAssetReferenceTryReadVersion(reader, kIncorrectVersion));
                using (var reader = new StreamBinaryReader(path))
                    Assert.IsTrue(BlobAssetReferenceTryReadVersion(reader, kVersion));*/
                builder.Dispose();
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        
        [Test]
        public void BlobAssetSerialization()
        {
            string path = "Packages/com.junk.core/Junk.Core.Creation.Tests/DataAssets/BlobAssetMeshGeometry.blob";
            if (File.Exists(path))
                File.Delete(path);
                
            try
            {
                using (var asset = BlobMesh.ToBlobData(referenceMesh))
                {
                    /*using (var writer = new StreamBinaryWriter(path))
                    {
                        writer.Write(asset);
                    }*/
                }
                
                /*using (var reader = new StreamBinaryReader(path))
                {
                    BlobAssetReference<BlobMesh> clone = reader.Read<BlobMesh>();
                    
                    ValidateBlobData(ref clone.Value, referenceMesh);
                    
                }*/
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
        
        
        bool BlobAssetReferenceTryReadVersion<T>(T reader, int version) where T : Unity.Entities.Serialization.BinaryReader
        {
            var result = BlobAssetReference<BlobMesh>.TryRead(reader, version, out var blobResult);
            if(result == false)
                return false;
            ValidateBlobData(ref blobResult.Value, referenceMesh);
            blobResult.Dispose();
            return true;
        }



        
    }
}