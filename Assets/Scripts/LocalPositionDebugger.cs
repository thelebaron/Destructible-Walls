using UnityEngine;

[ExecuteInEditMode]
public class LocalPositionDebugger : MonoBehaviour
{
    public int Index = 5;
    public float radius = 0.1f;
    public Vector3 pos;
    public Vector3 localVertPos;
    public Vector3 worldVertPos;
    public float rounded;

    // Update is called once per frame
    void Update()
    {
        pos = GetComponent<Renderer>().bounds.center;

        localVertPos = GetComponent<MeshFilter>().sharedMesh.vertices[Index];

        worldVertPos = transform.TransformPoint(localVertPos);

        var round = System.Math.Round(worldVertPos.x, 2);
        rounded = (float)round;

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(worldVertPos, radius);
    }
}
