using System;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class DebugAnchorSystem : SystemBase
{
    private AnchorDebug m_AnchorDebug;
    protected override void OnCreate()
    {
        base.OnCreate();
        var debugGo = new GameObject();
        m_AnchorDebug = debugGo.AddComponent<AnchorDebug>();
        
    }

    protected override void OnUpdate()
    {
        throw new NotImplementedException();
    }
}



public class AnchorDebug : MonoBehaviour
{
    public Mesh mesh;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
    }
}
