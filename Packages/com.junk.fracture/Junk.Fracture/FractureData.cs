using System;
using Junk.Collections;
using Unity.Collections;
using Unity.Entities;

namespace Junk.Fracture
{
    [Serializable]
    public struct FractureData
    {
        public bool UseAnchors;
    }
    
    [Serializable]
    public struct FractureGraphData
    {
        public BlobArray<Entity> Nodes;
    }
    
    [Serializable]
    public struct FractureConnectionMapData
    {
        // id -> index
        public BlobHashMap<int, int> MappingIndex;
        // index -> list of connections
        public BlobArray<BlobArray<int>> ConnectionMap;

    }
}