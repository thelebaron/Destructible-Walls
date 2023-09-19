using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Destroy
{
    [Serializable]
    public class NestedNodeTrabsformList
    {
        public List<Transform> myList;
        public Transform       AnchorTransform;
    }

    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour
    {
        [HideInInspector] public bool dirty = true;
        public                   bool isAnchor;

        public bool ShowConnections
        {
            get => _ShowConnections;
            set => _ShowConnections = value;
        }

        private static bool    _ShowConnections;
        public         Vector3 Position => Renderer.bounds.center;
        public         Mesh    Mesh     => MeshFilter.sharedMesh;

        private Renderer Renderer
        {
            get
            {
                if (m_Renderer == null)
                    m_Renderer = GetComponent<Renderer>();

                return m_Renderer;
            }
        }

        private Renderer m_Renderer;

        private MeshFilter MeshFilter
        {
            get
            {
                if (m_MeshFilter == null)
                    m_MeshFilter = GetComponent<MeshFilter>();

                return m_MeshFilter;
            }
        }

        private MeshFilter m_MeshFilter;

        public Transform                     Root => transform.root;
        public List<Transform>               anchors     = new();
        public List<Transform>               connections = new();
        public List<NestedNodeTrabsformList> nodeLinks   = new();


        public void AddNodeChain(NestedNodeTrabsformList list)
        {
            if (!nodeLinks.Contains(list))
                nodeLinks.Add(list);
        }

        public void OnDrawGizmosSelected()
        {
            //draw for connections

            if (isAnchor)
            {
                var anchorpos = GetComponent<Renderer>().bounds.center;

                Gizmos.color = Color.white;
                Gizmos.DrawCube(anchorpos, 0.55f * Vector3.one);
            }

            if (!isAnchor)
            {
                var anchorpos = GetComponent<Renderer>().bounds.center;

                Gizmos.color = Color.blue;
                //Gizmos.DrawMesh();
                Gizmos.DrawCube(anchorpos, 0.55f * Vector3.one);
            }

            if (connections.Count > 0)
                for (var i = 0; i < connections.Count; i++)
                {
                    Gizmos.color = Color.yellow;


                    var currentPos = connections[i].GetComponent<Renderer>().bounds.center;
                    Gizmos.DrawSphere(currentPos, 0.25f);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(Position, connections[i].GetComponent<NodeAuthoring>().Position);
                }

            if (_ShowConnections)
                if (nodeLinks.Count > 0)
                    foreach (var nodelink in nodeLinks)
                        // draw lines
                        for (var i = 0; i < nodelink.myList.Count; i++)
                        {
                            Gizmos.color = Color.yellow;
                            var currentPos = nodelink.myList[i].GetComponent<Renderer>().bounds.center;
                            Gizmos.DrawSphere(currentPos, 0.25f);

                            var nextindex = math.min(i + 1, nodelink.myList.Count - 1);
                            if (nextindex > nodelink.myList.Count)
                                nextindex = 0;
                            var nextPos = nodelink.myList[nextindex].GetComponent<Renderer>().bounds.center;

                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(currentPos, nextPos);

                            var nodeIsAnchor = nodelink.myList[i].GetComponent<NodeAuthoring>().isAnchor;
                            if (nodeIsAnchor)
                            {
                                var anchorpos = nodelink.myList[i].GetComponent<Renderer>().bounds.center;
                                Gizmos.color = Color.white;
                                Gizmos.DrawCube(anchorpos, 0.55f * Vector3.one);
                            }
                        }
        }
    }
}