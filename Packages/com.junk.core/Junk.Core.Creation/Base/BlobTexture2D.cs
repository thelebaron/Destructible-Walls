using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Junk.Core.Creation
{
    /// <summary>
    /// Data class to serialize a unityengine mesh to save friendly format.
    /// See https://docs.unity3d.com/ScriptReference/Mesh.html
    /// </summary>
    [Serializable]
    public struct BlobTexture2D
    {
        public BlobString name;
        public BlobArray<byte> rawData;
        public int height;
        public int width;
        private TextureFormat format;
        public TextureCreationFlags flags;
        private FilterMode filterMode;

        /// <summary>
        /// Must be manually disposed
        /// </summary>
        public static unsafe BlobBuilder ConstructBlobBuilder(Texture2D texture)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobTexture2D>();

            root.height = texture.height;
            root.width = texture.width;
            root.format = texture.format;
            root.filterMode = texture.filterMode;
            //root.flags = texture.format;
            
            builder.AllocateString(ref root.name, texture.name);
            
            byte[] byteArray = texture.GetRawTextureData();
            var raw = builder.Allocate<byte>(ref root.rawData, byteArray.Length);
            for (int i = 0; i < raw.Length; i++)
                raw[i] = byteArray[i];
            
            return builder;
        }
        
        public Texture2D ToMesh()
        {
            var texture2D = new Texture2D(height, width);

            texture2D.name        = name.ToString();
            texture2D.LoadRawTextureData(rawData.ToArray());
            //mesh.sub
            return texture2D;
        }
    }
}