using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LocalPositionDebugger : MonoBehaviour
{
    public Vector3 pos;

    // Update is called once per frame
    void Update()
    {
        pos = GetComponent<Renderer>().bounds.center;
    }
}
