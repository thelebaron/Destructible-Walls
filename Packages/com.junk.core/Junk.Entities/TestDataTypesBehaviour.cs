﻿using System.IO;
 using Junk.Core.Creation;
 using Unity.Entities;
 using Unity.Entities.Serialization;
using UnityEngine;
using BinaryWriter = System.IO.BinaryWriter;


 namespace Junk.Entities
 {
    public class TestDataTypesBehaviour : MonoBehaviour
    {
        public Mesh mesh;
        
        [ContextMenu("Test")]
        public void Test()
        {
            var pathjson = Application.dataPath + "\\" + "shapejson.json";
            var pathbinary  = Application.dataPath + "\\" + "shape.binary";
            var box    = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            var filter = box.GetComponent<MeshFilter>();

            var shape = new Shape(filter.sharedMesh);
            var json = JsonUtility.ToJson(shape, true);
            
            using (var writer = new StreamWriter(pathjson))
            {
                writer.Write(json);
            }

            var stream = File.OpenWrite(pathbinary);
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(json);
            }
            stream.Dispose();
            DestroyImmediate(box);
        }


        [ContextMenu("SerializeBlobGeometryTest")]
        public void BlobTest()
        {
            var path = Application.dataPath + "\\" + "geo.blob";

            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                var filter = box.GetComponent<MeshFilter>();
          
                
                //var stream = File.OpenWrite(path);
                
                using (var asset = BlobMesh.ToBlobData(filter.sharedMesh))
                {
                    /*using (var writer = new StreamBinaryWriter(path))
                    {
                        writer.Write(asset);
                    }*/
                    
                }
                DestroyImmediate(box);
            }
        }
        
        [ContextMenu("DESerializeBlobGeometryTest")]
        public void BlobDeserializeTest()
        {
            var path = Application.dataPath + "\\" + "geo.blob";

            {
                var box    = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var filter = box.GetComponent<MeshFilter>();
                box.name = "DeserializedBlobGeometry";
                filter.sharedMesh = null;
                filter.mesh = null;
                
                
                
                /*var reader = new StreamBinaryReader(path);
                BlobAssetReference<BlobMesh> clone = reader.Read<BlobMesh>();

                Debug.Log(clone.Value.vertices.Length);
                for (int i = 0; i < clone.Value.vertices.Length; i++)
                {
                    var v = clone.Value.vertices[i];
                    Debug.Log(v);
                }
                
                var mesh = clone.Value.ToMesh();
                filter.mesh = mesh;
                filter.sharedMesh = mesh;
                clone.Dispose();
                reader.Dispose();
                */
            }
        }

        
        /*/// <summary>
        /// Writes the blob data to a path with serialized version.
        /// </summary>
        /// <param name="builder">The BlobBuilder containing the blob to write.</param>
        /// <param name="path">The path to write the blob data.</param>
        /// <param name="version">Serialized version number of the blob data.</param>
        public static void Write(BlobBuilder builder, string path, int verison)
        {
            using (var asset = builder.CreateBlobAssetReference<T>(Allocator.TempJob))
                using (var writer = new StreamBinaryWriter(path))
                {
                    writer.Write(verison);
                    writer.Write(asset);
                }
        }*/
    }
}