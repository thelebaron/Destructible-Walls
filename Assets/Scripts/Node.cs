using UnityEngine;

public class NodeInfo : MonoBehaviour
{
    public Vector3    startPosition;
    public Quaternion startRotation;
    public Bounds     startBounds;

    
    void Reset()
    {
        var meshRenderer = this.GetComponent<MeshRenderer>();
        
        startBounds   = meshRenderer.bounds;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void UpdatePreview(float distance)
    {
        transform.position = transform.parent.TransformPoint(startBounds.center * distance);
    }
}