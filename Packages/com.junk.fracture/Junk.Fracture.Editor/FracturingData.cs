using System.Collections.Generic;
using Junk.Entities;
using Junk.Fracture.Hybrid;
using Unity.Entities;
using UnityEngine;

namespace Junk.Fracture.Editor
{        
    public enum FractureType
    {
        Voronoi,
        Clustered,
        Slicing
    }
    
    public class FracturingData
    {
        public UnityObjectRef<Object> targetObject;
        public UnityObjectRef<Mesh>   targetMesh;
        public bool                   applyToObject;
        public List<GameObject>       fractureList;
        public string                 name;
        
        public Object                 target;
        //public FractureCache          cache;
        public string                 labelText = "";

        // voronoi settings
        public float density     = 500;
        public int   totalChunks = 20;

        // clustered settings
        public int   clusters        = 5;
        public int   sitesPerCluster = 5;
        public float clusterRadius   = 1;

        // slicing settings
        public Vector3Int slices            = Vector3Int.one;
        public int        offset_variations = 0;
        public int        angle_variations  = 0;
        public float      amplitude         = 0;
        public float      frequency         = 1;
        public int        octaveNumber      = 1;
        public int        surfaceResolution = 2;

        public FractureType fractureType = FractureType.Voronoi;

        public int      seed = -1;
        public float    breakForce = 100;

        public DestructibleAuthoring authoring;

        public NvFractureData nvFractureData;
        public Mesh           meshAsset;
        public Material       materialAsset;
        public Material[]     materialAssets;
        public Material       insideMaterial;
        public Material       outsideMaterial;
    }
}