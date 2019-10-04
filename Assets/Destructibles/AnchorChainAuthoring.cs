using System.Collections.Generic;
using UnityEngine;

namespace Destructibles
{
    public class AnchorChainAuthoring : MonoBehaviour
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

        public List<Transform> tempList = new List<Transform>();

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
                            tempList.Add(currentTransform);
                        }
                    }
                }
            }
        }
    }
}