using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaosEditorDebugInfo : MonoBehaviour
{
    public List<NodeList> nodes;
}

[Serializable]
public class NodeList
{
    public List<NodeInfo> nodes;
}