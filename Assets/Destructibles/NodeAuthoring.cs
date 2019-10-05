using System;
using System.Collections.Generic;
using System.Linq;
using thelebaron.Damage;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

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
                // Create Node Chains
                foreach (var nodeChain in nodeLinks)
                {
                    Debug.Log("link" + gameObject.name);
                    var e = dstManager.CreateEntity();

                    var buffer = dstManager.AddBuffer<GraphLink>(e);

                    foreach (var tr in nodeChain.myList)
                    {
                        buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                    }
            
                    dstManager.SetName(e, "Graph Link");
                    
                    //if(!dstManager.HasComponent<Node>(e))
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
            
            //Debug.Log("Added list");
        }
/*        public void AddNodeChain(List<Transform> list)
        {
            if(!nodeChains.Contains(list))
                nodeChains.Add(list);
            
            Debug.Log("Added list");
        }*/
    }

    public struct noderoot : IComponentData
    {
        public Entity Value;
    }
}