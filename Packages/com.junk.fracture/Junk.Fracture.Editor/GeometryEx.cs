using System.Collections.Generic;
using System.Linq;
using Junk.Fracture.Hybrid;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Fracture.Editor
{
    public static class GeometryEx
    {
        public static void SetPreviewDistance(GameObject gameObject, float distance)
        {
            // Get the current GameObject's MeshRenderer bounds center
            MeshRenderer currentMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (currentMeshRenderer == null) 
                return;

            Vector3 center = currentMeshRenderer.bounds.center;

            var chunks = gameObject.GetComponentsInChildren<ModelChunkInfo>();
            if(chunks.Length<1)
                return;

            // Draw a line from this GameObject to each connected GameObject
            foreach (var other in chunks)
            {
                if (other == null) 
                    continue;

                if (distance.Equals(0))
                {
                    //other.transform.position = Vector3.zero;
                    //continue;
                }
        
                var mr = other.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Vector3 currentCenter = mr.bounds.center;

                    // Calculate the direction from the center to the current chunk
                    Vector3 direction = (currentCenter - center).normalized;
            
                    // Set the new position based on the direction and distance
                    other.transform.position = center + direction * distance;
                }
            }
        }
        
        public static void ResetPreviewDistance(GameObject gameObject)
        {
            var chunks = gameObject.GetComponentsInChildren<Transform>().ToList();
            if(chunks.Count < 1)
                return;
            
            if(chunks.Contains(gameObject.transform))
                chunks.Remove(gameObject.transform);
            
            // Draw a line from this GameObject to each connected GameObject
            foreach (var other in chunks)
            {
                if (other.GetComponent<ModelChunkInfo>()!=null)
                    other.transform.localPosition = Vector3.zero;
            }
        }
        
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