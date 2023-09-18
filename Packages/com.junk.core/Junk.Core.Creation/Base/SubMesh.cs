using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.Core.Creation
{
    /// <summary>
    /// Keep parity with SubMeshDescriptor
    /// </summary>
    [Serializable]
    public struct SubMesh
    {
        public BlobMesh.Bounds bounds;
        public MeshTopology topology;
        public int indexStart;
        public int indexCount;
        public int baseVertex;
        public int firstVertex;
        public int vertexCount;

        public override string ToString()
        {
            return string.Format("(topo={0} indices={1},{2} vertices={3},{4} basevtx={5} bounds={6})", (object) this.topology, (object) this.indexStart, (object) this.indexCount, (object) this.firstVertex, (object) this.vertexCount, (object) this.baseVertex, (object) this.bounds);
        }
        
        public static implicit operator SubMeshDescriptor(SubMesh s)
        {
            return new SubMeshDescriptor
            {
                bounds = s.bounds, 
                topology = s.topology, 
                indexStart = s.indexStart, 
                indexCount = s.indexCount, 
                baseVertex = s.baseVertex, 
                firstVertex = s.firstVertex, 
                vertexCount = s.vertexCount
            };
        }

        public static implicit operator SubMesh(SubMeshDescriptor s)
        {
            return new SubMesh
            {
                bounds      = s.bounds, 
                topology    = s.topology, 
                indexStart  = s.indexStart, 
                indexCount  = s.indexCount, 
                baseVertex  = s.baseVertex, 
                firstVertex = s.firstVertex, 
                vertexCount = s.vertexCount
            };
        }
    }
}