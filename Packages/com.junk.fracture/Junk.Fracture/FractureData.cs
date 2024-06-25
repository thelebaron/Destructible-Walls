using System;
using Junk.Collections;
using Unity.Entities;

namespace Junk.Fracture
{
    [Serializable]
    public struct FractureData
    {
        public bool                                           UseAnchors;
        public BlobHashMap<int, BlobHashMap<int, BlobArray<int>>> PathLookup;
        //Dictionary<Entity, Dictionary<Entity, List<Entity>>>();
    }
    
    [Serializable]
    public struct FractureGraphData
    {
        public BlobArray<Entity> Nodes;
    }
}