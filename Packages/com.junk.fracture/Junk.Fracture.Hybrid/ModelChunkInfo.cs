using System;
using System.Collections.Generic;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    /// <summary>
    /// Serialized data during the fracture setup. This isnt used in gameplay situations, only for DCC use.
    /// </summary>
    public class ModelChunkInfo : MonoBehaviour
    {
        public string           ChunkSeries;
        public string           ChunkIndex;
        public List<GameObject> Connected;

        public void OnDrawGizmosSelected()
        {
            if (Connected == null) return;

            // Get the current GameObject's MeshRenderer bounds center
            MeshRenderer currentMeshRenderer = GetComponent<MeshRenderer>();
            if (currentMeshRenderer == null) return;
        
            Vector3 currentCenter = currentMeshRenderer.bounds.center;

            // Draw a line from this GameObject to each connected GameObject
            foreach (GameObject connectedObject in Connected)
            {
                if (connectedObject != null)
                {
                    MeshRenderer connectedMeshRenderer = connectedObject.GetComponent<MeshRenderer>();
                    if (connectedMeshRenderer != null)
                    {
                        Vector3 connectedCenter = connectedMeshRenderer.bounds.center;
                        Gizmos.color = Color.green; // Set the color of the line
                        Gizmos.DrawLine(currentCenter, connectedCenter);
                    }
                }
            }
        }
    }
}
