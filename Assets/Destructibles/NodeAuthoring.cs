using System;
using System.Collections.Generic;
using thelebaron.Damage;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Destructibles
{
    [System.Serializable]
    public class NestedNodeTrabsformList
    {
        public List<Transform> myList;
        public Transform AnchorTransform;
    }
    
    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        [HideInInspector] public bool dirty = true;
        public bool isAnchor;
        public Vector3 Position => Renderer.bounds.center;
        public Mesh Mesh => MeshFilter.sharedMesh;

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
        
        public Transform Root => transform.root;
        public List<Transform> anchors = new List<Transform>();
        public List<Transform> connections = new List<Transform>();
        public List<NestedNodeTrabsformList> nodeLinks =new List<NestedNodeTrabsformList>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BreakableNode());
            dstManager.AddComponentData(entity, new Health {Value = 10, Max = 10});
            dstManager.SetName(entity, "Breakable node " + name);
            
            {
                // Get the root graph 
                var graph = conversionSystem.GetPrimaryEntity(transform.parent);

                // If considered a static anchor
                if(isAnchor)
                    dstManager.AddComponentData(entity, new AnchorNode());
                
                var nodeNeighbors = dstManager.AddBuffer<NodeNeighbor>(entity);
                for (int i = 0; i < connections.Count; i++)
                {
                    var otherentity = conversionSystem.GetPrimaryEntity(connections[i]);

                    nodeNeighbors.Add(otherentity);
                    foreach (var neighbor in nodeNeighbors)
                    {
                        if (neighbor.Node.Equals(entity))
                        {
                            Debug.Log("Adding self?!");
                        }
                            
                    }
                    
                }

                
                // Add all neighbor nodes 
                var connectionGraph = dstManager.GetBuffer<ConnectionGraph>(graph);
                connectionGraph.Add(entity);
                
                // Add all anchors
                foreach (var tr in anchors)
                {
                    var anchorEntity = conversionSystem.GetPrimaryEntity(tr);
                    var hasEntity = false;
                    
                    // Do lookup for buffer
                    if (!dstManager.HasComponent(entity, typeof(NodeAnchorBuffer)))
                    {
                        var buffer = dstManager.AddBuffer<NodeAnchorBuffer>(entity);
                        
                        // Dont add if contains
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (buffer[i].Node.Equals(anchorEntity))
                                hasEntity = true;
                        }
                        if(!hasEntity)
                            buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                    }

                    if (dstManager.HasComponent(entity, typeof(NodeAnchorBuffer)))
                    {
                        var buffer = dstManager.GetBuffer<NodeAnchorBuffer>(entity);
                        
                        // Dont add if contains
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            if (buffer[i].Node.Equals(anchorEntity))
                                hasEntity = true;
                        }
                        if(!hasEntity)
                            buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                    }
                }
                
            }



            {
                dstManager.AddBuffer<NodeLinkBuffer>(entity);
                
                // Create Node Links
                foreach (var nodeChain in nodeLinks)
                {
                    //Debug.Log("link" + gameObject.name);
                    var e = dstManager.CreateEntity();
                    
                    /*
                    if (dstManager.HasComponent(entity, typeof(NodeLinkBuffer)))
                    {
                        var linkBuffers = dstManager.GetBuffer<NodeLinkBuffer>(entity);
                        linkBuffers.Add(e);
                    }
                    */
                    var buffer = dstManager.AddBuffer<GraphLink>(e);

                    foreach (var tr in nodeChain.myList)
                    {
                        buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                    }
            
                    dstManager.SetName(e, "Graph Link");
                    
                    dstManager.AddComponentData(e, new GraphNode
                    {
                        Node = entity
                    });
                    dstManager.AddComponentData(e, new GraphAnchor
                    {
                        Node = conversionSystem.GetPrimaryEntity(nodeChain.AnchorTransform)
                    });

                    
                    /*
                    if (!dstManager.HasComponent(e, typeof(NodeAnchorBuffer)))
                    {
                        dstManager.AddBuffer<NodeAnchorBuffer>(e);
                    }

                    if (dstManager.HasComponent(e, typeof(NodeAnchorBuffer)))
                    {
                        var anchor = dstManager.GetBuffer<NodeAnchorBuffer>(e);
                    }*/
                }

            }
        }


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                referencedPrefabs.Add(connections[i].gameObject);
            }
        }

        public void AddNodeChain(NestedNodeTrabsformList list)
        {
            if(!nodeLinks.Contains(list))
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
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    Gizmos.color = Color.yellow;
                    
                    
                    
                    
                    
                    var currentPos = connections[i].GetComponent<Renderer>().bounds.center;
                    Gizmos.DrawSphere(currentPos, 0.25f);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(Position,connections[i].GetComponent<NodeAuthoring>().Position);
                    
                }
                
            }
            
            
             // Draw for nodelink
            if (isAnchor)
            {
                var anchorpos = GetComponent<Renderer>().bounds.center;
                
                Gizmos.color = Color.red;
                Gizmos.DrawCube(anchorpos, 0.55f * Vector3.one);
            }
            if (nodeLinks.Count > 0)
            {
                foreach (var nodelink in nodeLinks)
                {
                    foreach (var item in nodelink.myList)
                    {
                        
                    }
                    
                    // draw lines
                    for (int i = 0; i < nodelink.myList.Count; i++)
                    {
                        Gizmos.color = Color.yellow;
                        var currentPos = nodelink.myList[i].GetComponent<Renderer>().bounds.center;
                        Gizmos.DrawSphere(currentPos, 0.25f);

                        var nextindex = math.min(i + 1, nodelink.myList.Count -1);
                        if (nextindex > nodelink.myList.Count)
                            nextindex = 0;
                        var nextPos = nodelink.myList[nextindex].GetComponent<Renderer>().bounds.center;
                        
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(currentPos,nextPos);
                    }
                }
                
            }
        }
    }

    
}