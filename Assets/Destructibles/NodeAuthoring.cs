using System.Collections.Generic;
using thelebaron.Damage;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Destructibles
{

    [DisallowMultipleComponent]
    public class NodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public bool isAnchor;
        public Transform Root => transform.root;
        public List<Transform> anchors = new List<Transform>();
        public List<Transform> connections = new List<Transform>();
        public List<Transform> anchorChain = new List<Transform>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            {
                // Get the root graph 
                var graph = conversionSystem.GetPrimaryEntity(transform.parent);

                // If considered a static anchor
                if(isAnchor)
                    dstManager.AddComponentData(entity, new StaticAnchor());
                
                // Add node and set the fracture graph entity
                dstManager.AddComponentData(entity, new Node{ Graph = graph});
                
                var connectionJoints = dstManager.AddBuffer<Neighbors>(entity);
                foreach (var j in connections)
                {
                    var otherentity = conversionSystem.GetPrimaryEntity(j.gameObject);

                    connectionJoints.Add(otherentity);
                }
                
                // Add each 
                var connectionGraph = dstManager.GetBuffer<ConnectionGraph>(graph);
                connectionGraph.Add(entity);
            }
            

            var anchorChain = new List<Transform>();
            AddRecurse(anchorChain);
            
            //AddGraphRecurse(dstManager, conversionSystem, transform, convertedTransformList);














            {
                // Add the node buffer
                dstManager.AddComponentData(entity, new Health {Value = 10, Max = 10});
                // Todo evaluate if necessary?
                dstManager.AddComponentData(entity, new Anchored());
                dstManager.AddComponentData(entity, new DynamicAnchor());

                dstManager.SetName(entity, "FractureNode_" + name);
            }
        }

        private void AddRecurse(List<Transform> anchorChainTransform)
        {
            
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                referencedPrefabs.Add(connections[i].gameObject);
            }
        }
        
        public static void AddGraphRecurse(EntityManager manager, GameObjectConversionSystem conversionSystem,
            Transform tr, List<Transform> convertedTransformList)
        {
            if(!convertedTransformList.Contains(tr))
                convertedTransformList.Add(tr);
            
            var entity = conversionSystem.GetPrimaryEntity(tr);
            manager.AddComponentData(entity, new Anchored());
            var nodeChildren = new DynamicBuffer<NodeChild>();
            
            if (!manager.HasComponent<NodeChild>(entity))
            {
                nodeChildren = manager.AddBuffer<NodeChild>(entity);
            }
            if (manager.HasComponent<NodeChild>(entity))
            {
                nodeChildren = manager.GetBuffer<NodeChild>(entity);
            }
            
            // Loop through connected nodes
            var node = tr.GetComponent<NodeAuthoring>();
            if (node != null && node.connections.Count > 0)
            {
                foreach (Transform childTransform in node.connections)
                {
                    // Add connection child to graph but dont add duplicates
                    var childEntity = conversionSystem.GetPrimaryEntity(childTransform);
                    var isDuplicate = false;
                    foreach (var nodechild in nodeChildren)
                    {
                        if (nodechild.Node.Equals(childEntity))
                            isDuplicate = true;
                    }
                    
                    if(!isDuplicate)
                        nodeChildren.Add(childEntity);

                    if(!manager.HasComponent<NodeParent>(childEntity))
                        manager.AddComponentData(childEntity, new NodeParent{ Node = entity });
                    
                    if (!convertedTransformList.Contains(childTransform))
                        AddGraphRecurse(manager, conversionSystem, childTransform, convertedTransformList);
                    
                    
                }
            }
            
            
        }
        

    }
}