using System.Collections.Generic;
using Junk.Fracture.Hybrid;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Fracture.Editor
{
    public static class GeometryEx
    {
        public static void GetOverlaps(List<(GameObject, Mesh)> chunks, float touchRadius = 0.01f)
        {
            foreach (var chunk in chunks)
            {
                var gameObject = chunk.Item1;
                var mesh       = chunk.Item2;

                if (gameObject.GetComponent<ModelChunkInfo>().Connected == null)
                    gameObject.GetComponent<ModelChunkInfo>().Connected = new List<GameObject>();
                
                var buffer = gameObject.GetComponent<ModelChunkInfo>().Connected;
                
                // get mesh data to calculate overlaps
                var triangles = mesh.GetTriangles(1);
                        
                foreach (var tri in triangles)
                {
                    var vertex        = mesh.vertices[tri];
                    var     worldPosition = math.transform(float4x4.TRS(gameObject.transform.position, gameObject.transform.rotation, 1), vertex);

                    foreach (var other in chunks)
                    {
                        var otherEntity = other.Item1;
                        var otherMesh   = other.Item2;
                        var otherTriangles = otherMesh.GetTriangles(1);
                        
                        foreach (var otherTri in otherTriangles)
                        {
                            var otherVertex   = otherMesh.vertices[otherTri];
                            var otherPosition = math.transform(float4x4.TRS(otherEntity.transform.position, otherEntity.transform.rotation, 1), otherVertex);
                            
                            if (!(Vector3.Distance(worldPosition, otherPosition) <= touchRadius)) 
                                continue;
                            
                            if (otherEntity != gameObject && !buffer.Contains(otherEntity))
                            {
                                buffer.Add(otherEntity);
                            }
                        }
                    }
                }
            }
        }

        public static int[] GetTriangle(Mesh mesh, int submeshIndex)
        {
            
            // Get the indices of the submesh
            int[] submeshIndices = mesh.GetTriangles(submeshIndex);
            
            return submeshIndices;

            // Iterate through the indices and access the corresponding vertices
            foreach (int index in submeshIndices)
            {
                Vector3 vertex = mesh.vertices[index];
                Debug.Log($"Vertex Index: {index}, Position: {vertex}");
            }
            
            
        }
    }
}