using System;
using System.Collections.Generic;
using thelebaron.Damage;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Destructibles
{
    [System.Serializable]
    public class NestedTransformList
    {
        public List<Transform> myList;
    }
    
    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public bool isAnchor;
        public Transform Root => transform.root;
        public List<Transform> anchors = new List<Transform>();
        public List<Transform> connections = new List<Transform>();
        //public List<List<Transform>> nodeChains = new List<List<Transform>>();
        public List<NestedTransformList> nodeChains =new List<NestedTransformList>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            
            dstManager.AddComponentData(entity, new NodeBreakable
            {
                Value = entity
            });
            
            {
                // Get the root graph 
                var graph = conversionSystem.GetPrimaryEntity(transform.parent);

                // If considered a static anchor
                if(isAnchor)
                    dstManager.AddComponentData(entity, new StaticAnchor());
                
                // Add node and set the fracture graph entity
                //dstManager.AddComponentData(entity, new Node{ Graph = graph});
                
                var connectionJoints = dstManager.AddBuffer<NodeNeighbor>(entity);
                for (int i = 0; i < connections.Count; i++)
                {
                    var otherentity = conversionSystem.GetPrimaryEntity(connections[i]);

                    connectionJoints.Add(otherentity);
                }
                /*
                foreach (var j in connections)
                {
                    var otherentity = conversionSystem.GetPrimaryEntity(j.gameObject);

                    connectionJoints.Add(otherentity);
                }*/
                
                // Add each 
                var connectionGraph = dstManager.GetBuffer<ConnectionGraph>(graph);
                connectionGraph.Add(entity);
            }
            
            

            {
                // Add the node buffer
                dstManager.AddComponentData(entity, new Health {Value = 10, Max = 10});
                // Todo evaluate if necessary?
                dstManager.AddComponentData(entity, new Anchored());
                dstManager.AddComponentData(entity, new DynamicAnchor());

                dstManager.SetName(entity, "FractureNode_" + name);
            }


            {
                //nodeChains = GetComponents<NodeChain>();
                
                foreach (var nodeChain in nodeChains)
                {
                    var e = dstManager.CreateEntity();

                    var buffer = dstManager.AddBuffer<Chain>(e);

                    foreach (var tr in nodeChain.myList)
                    {
                        buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                    }
            
                    dstManager.SetName(e, "nodeChain");
                    
                    //if(!dstManager.HasComponent<Node>(e))
                    dstManager.AddComponentData(e, new Node
                    {
                        Value = entity
                    });

                    if (!dstManager.HasComponent(e, typeof(Anchor)))
                    {
                        dstManager.AddBuffer<Anchor>(e);
                    }

                    if (dstManager.HasComponent(e, typeof(Anchor)))
                    {
                        var anchor = dstManager.GetBuffer<Anchor>(e);
                        
                        
                        //anchor.Add()
                    }
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

        public void AddNodeChain(NestedTransformList list)
        {
            if(!nodeChains.Contains(list))
                nodeChains.Add(list);
            
            Debug.Log("Added list");
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