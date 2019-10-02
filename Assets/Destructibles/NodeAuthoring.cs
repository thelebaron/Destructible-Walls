using System.Collections.Generic;
using thelebaron.Damage;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Serialization;

namespace Destructibles
{


    public struct Node : IComponentData
    {
    }

    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public bool isAnchor;
        private GameObject RootGameObject;
        [FormerlySerializedAs("Connections")] public List<Transform> NodeConnections = new List<Transform>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if(isAnchor)
                dstManager.AddComponentData(entity, new StaticAnchor());
            
            var rootEntity = conversionSystem.GetPrimaryEntity(transform.parent);
            var connectionJoints = dstManager.AddBuffer<Connection>(entity);
            //var graphChildBuffer = dstManager.AddBuffer<GraphChild>(entity);
            
            foreach (var t in NodeConnections)
            {
                var otherentity = conversionSystem.GetPrimaryEntity(t.gameObject);

                connectionJoints.Add(otherentity);
            }

            AddGraphRecurse(dstManager, conversionSystem, transform);

            // Add each 
            var connectionGraph = dstManager.GetBuffer<ConnectionGraph>(rootEntity);
            connectionGraph.Add(entity);


            // Add the node buffer
            dstManager.AddComponentData(entity, new Health {Value = 10, Max = 10});
            dstManager.AddComponentData(entity, new Node());
            dstManager.AddComponentData(entity, new DynamicAnchor());

            dstManager.SetName(entity, "FractureNode_" + name);
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            for (int i = 0; i < NodeConnections.Count; i++)
            {
                referencedPrefabs.Add(NodeConnections[i].gameObject);
            }
        }
        
        public static void AddGraphRecurse(EntityManager manager, GameObjectConversionSystem conversionSystem, Transform tr)
        {
            var entity = conversionSystem.GetPrimaryEntity(tr);
            var graph = new DynamicBuffer<GraphChild>();
            
            if (!manager.HasComponent<GraphChild>(entity))
            {
                graph = manager.AddBuffer<GraphChild>(entity);
            }
            if (manager.HasComponent<GraphChild>(entity))
            {
                graph = manager.GetBuffer<GraphChild>(entity);
            }
            
            // Loop through connected nodes
            var node = tr.GetComponent<NodeAuthoring>();
            if (node != null && node.NodeConnections.Count > 0)
            {
                foreach (Transform child in node.NodeConnections)
                {
                    // Add connection child to graph but dont add duplicates
                    var isDuplicate = false;
                    foreach (var graphChild in graph)
                    {
                        if (graphChild.Node.Equals(conversionSystem.GetPrimaryEntity(child)))
                            isDuplicate = true;
                    }
                    if(!isDuplicate)
                        graph.Add(conversionSystem.GetPrimaryEntity(child));

                    if(!manager.HasComponent<NodeParent>(conversionSystem.GetPrimaryEntity(child)))
                        manager.AddComponentData(conversionSystem.GetPrimaryEntity(child), new NodeParent{ Value = entity });
                    
                    AddGraphRecurse(manager, conversionSystem, child);
                }
            }
            
            /*
            var convert = tr.GetComponent<ConvertToEntity>();
            if (convert != null && convert.ConversionMode == ConvertToEntity.Mode.ConvertAndInjectGameObject)
                return;
                
            foreach (Transform child in tr)
                AddGraphRecurse(manager, conversionSystem, child);*/
        }
        

    }
}