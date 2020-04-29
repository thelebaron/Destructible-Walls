using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase, DisallowMultipleComponent]
public class FractureData : MonoBehaviour
{
    public List<NodeList> nodes;
    
    public List<NodeInfo> nodes1;
    public List<NodeInfo> nodes2;
    public List<NodeInfo> nodes3;
    public List<NodeInfo> nodes4;


    public void Reset()
    {
        
    }
}

[Serializable]
public class NodeList
{
    public List<NodeInfo> nodes;
}

public class FractureDataStructure
{
    
}