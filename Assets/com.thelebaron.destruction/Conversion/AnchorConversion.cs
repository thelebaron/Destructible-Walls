using System.Collections.Generic;
using thelebaron.Destruction;
using UnityEngine;

namespace thelebaron.Destruction.Authoring
{
    public static class AnchorConversion
    {
        public static void FindAnchors(BakeData bake)
        {
            // Then get all anchors, add to list and distribute to all nodes
            bake.nodes = bake.gameObject.GetComponentsInChildren<NodeAuthoring>();
            ConnectUnconnectedNodes(bake);
            
            var anchorNodes = new List<Transform>();
            
            foreach (var node in bake.nodes)
            {
                //Reset any node anchor lists
                node.nodeLinks = new List<NestedNodeTrabsformList>();
                
                if(node.isAnchor && !anchorNodes.Contains(node.transform))
                    anchorNodes.Add(node.transform);
            }
            
            foreach (var node in bake.nodes)
            {
                node.anchors = anchorNodes;
                CreateAnchorConnectivityMap(bake, node);
            }
        }
        
        
        /// <summary>
        /// Connects any nodes that didnt get connected initially
        /// </summary>
        private static void ConnectUnconnectedNodes(BakeData bake)
        {
            foreach (var node in bake.nodes)
            {
                if (node.connections.Count != 0) 
                    continue;
                
                foreach (var subnode in bake.nodes)
                {
                    if(subnode.connections.Contains(node.transform) && !node.connections.Contains(subnode.transform))
                        node.connections.Add(subnode.transform);
                }
            }
            
        }
        
        
        /// <summary>
        /// Bit of a recursive hell but: find all nodes connecting to an anchor
        /// </summary>
        private static void CreateAnchorConnectivityMap(BakeData bake, NodeAuthoring node)
        {
            foreach (var anchor in node.anchors)
            {
                var unused = new List<Transform>();

                Find(bake, anchor, node,node, new List<Transform>(), 0);
            }
        }
        
        
        /// <summary>
        /// recursively find
        /// </summary>
        private static void Find(BakeData bake, Transform anchor, NodeAuthoring node, NodeAuthoring searchNode, List<Transform> list, int iterations)
        {
            const int max = 9999;
            iterations++;
            if (iterations >= max)
                return ;
            
            foreach (var connection in searchNode.connections)
            {
                if (connection == anchor)
                {
                    if (!list.Contains(connection))
                    {
                        list.Add(connection);
                        var chainAuthoring = node.gameObject.AddComponent<NodeChain>();
                        chainAuthoring.actuallyFoundAnchor = true;
                        chainAuthoring.AnchorList          = list;
                        chainAuthoring.AnchorTransform     = connection;
                        chainAuthoring.Nodes               = bake.nodes;
                        chainAuthoring.Node                = node;
                        chainAuthoring.m_Connections       = node.connections;
                        chainAuthoring.ValidateList();
                    }
                    break;
                }

                if (list.Contains(connection)) 
                    continue;
                
                list.Add(connection);
                Find(bake, anchor, node, connection.GetComponent<NodeAuthoring>(), list, iterations);
            }
        }
    }
    
}