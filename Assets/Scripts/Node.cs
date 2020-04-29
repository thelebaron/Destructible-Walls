using System;
using UnityEngine;

[Serializable]
public class NodeInfo
{
    public Vector3    startPosition;
    public Quaternion startRotation;
    public Bounds     startBounds;

    void Reset(Transform transform)
    {
        var meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();
        
        startBounds   = meshRenderer.bounds;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void UpdatePreview(float distance, Transform transform)
    {
        transform.position = transform.parent.TransformPoint(startBounds.center * distance);
    }
}

public class NodeInfoBehaviour : MonoBehaviour
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