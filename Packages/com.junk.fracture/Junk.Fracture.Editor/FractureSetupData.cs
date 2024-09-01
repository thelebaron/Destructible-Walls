using Junk.Fracture.Hybrid;
using UnityEngine;

namespace Junk.Fracture.Editor
{        
    public enum FractureType
    {
        Voronoi,
        Clustered,
        Slicing
    }
    
    public class FractureSetupData
    {
        public Object        target;
        public FractureCache cache;
        public string        labelText = "";

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
        public string         name;
        public Mesh           meshAsset;
        public Material       materialAsset;
        public Material[]     materialAssets;
        public Material       insideMaterial;
        public Material       outsideMaterial;
    }
}