using System;
using Junk.Fracture.Hybrid;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    #if UNITY_EDITOR
    /// <summary> Object data that gets passed around during mesh conversion </summary>
    [Serializable]
    public class NvFractureData
    {
        //public FractureCache  RootCache;
        public int                seed = 3;
        public int                totalChunks = 20;
        public Mesh               mesh;
        public Material           insideMaterial;
        public Material           outsideMaterial;
    }
    #endif
}