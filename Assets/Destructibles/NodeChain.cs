using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Destructibles
{
    public class NodeChain : MonoBehaviour 
    {
        public NodeAuthoring Node;
        public bool actuallyFoundAnchor;
        public Transform AnchorTransform;
        [Space] public List<Transform> AnchorList;

        public NodeAuthoring[] Nodes
        {
            get => m_Nodes;
            set => m_Nodes = value;
        }

        public NodeAuthoring[] m_Nodes;
        public List<Transform> m_Connections = new List<Transform>();

        public List<Transform> validList = new List<Transform>();

        public void ValidateList()
        {
            if (AnchorTransform == transform)
            {
                var finalList = new List<Transform>();
                finalList.Add(transform);
                AnchorList = finalList;
                return;
            }
            else
            {
                //tempList = new List<Transform>();
                for (int i = 0; i < AnchorList.Count; i++)
                {
                    var next = i + 1;
                    if (next < AnchorList.Count)
                    {
                        var currentTransform = AnchorList[i];
                        var nextTransform = AnchorList[next];
                        var nConnections = AnchorList[i].GetComponent<NodeAuthoring>().connections;

                        if (!nConnections.Contains(currentTransform))
                        {
                            validList.Add(currentTransform);
                        }
                    }
                }
            }
            
            // List doesnt contain anchor in previous search, feels hacky to put here
            validList.Add(AnchorTransform);
            
            var chainnode = new NestedNodeTrabsformList {myList = validList, AnchorTransform = AnchorTransform};

            Node.AddNodeChain(chainnode);
            DestroyImmediate(this);
        }

        
    }
}