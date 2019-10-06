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
                
                var connectionJoints = dstManager.AddBuffer<NodeNeighbor>(entity);
                for (int i = 0; i < connections.Count; i++)
                {
                    var otherentity = conversionSystem.GetPrimaryEntity(connections[i]);

                    connectionJoints.Add(otherentity);
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

                        var nextindex = i + 1;
                        if (nextindex > nodelink.myList.Count)
                            nextindex = 0;
                        var nextPos = nodelink.myList[nextindex].GetComponent<Renderer>().bounds.center;
                        //Gizmos.DrawSphere(nextPos, 0.25f);

                        //var dist = math.distance(currentPos, nextPos);
                        
                        
                        //Gizmos.DrawSphere(item.GetComponent<Renderer>().bounds.center, 0.25f);
                        
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(currentPos,nextPos);
                    }
                }
                
            }
        }
    }

    
}