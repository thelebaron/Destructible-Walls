using Junk.Hitpoints;
using Junk.Destroy;
using Unity.Entities;

public class NodeBaker : Baker<NodeAuthoring>
{
    public override void Bake(NodeAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new BreakableNode());
        AddComponent(entity, new HealthData { Value = 10 });

        foreach (var connection in authoring.connections)
            CreateAdditionalEntity(TransformUsageFlags.Dynamic, false, connection.name);

        /*{
            // Get the root graph 
            var graph = conversionSystem.GetPrimaryEntity(transform.parent);

            // If considered a static anchor
            if (isAnchor)
                dstManager.AddComponentData(entity, new AnchorNode());

            var nodeNeighbors = dstManager.AddBuffer<NodeNeighbor>(entity);
            for (var i = 0; i < connections.Count; i++)
            {
                var otherentity = conversionSystem.GetPrimaryEntity(connections[i]);

                nodeNeighbors.Add(otherentity);
                foreach (var neighbor in nodeNeighbors)
                    if (neighbor.Node.Equals(entity))
                        Debug.Log("Adding self?!");
            }


            // Add all neighbor nodes 
            var connectionGraph = dstManager.GetBuffer<ConnectionGraph>(graph);
            connectionGraph.Add(entity);

            // Add all anchors
            foreach (var tr in anchors)
            {
                var anchorEntity = conversionSystem.GetPrimaryEntity(tr);
                var hasEntity    = false;

                // Do lookup for buffer
                if (!dstManager.HasComponent(entity, typeof(NodeAnchorBuffer)))
                {
                    var buffer = dstManager.AddBuffer<NodeAnchorBuffer>(entity);

                    // Dont add if contains
                    for (var i = 0; i < buffer.Length; i++)
                        if (buffer[i].Node.Equals(anchorEntity))
                            hasEntity = true;
                    if (!hasEntity)
                        buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                }

                if (dstManager.HasComponent(entity, typeof(NodeAnchorBuffer)))
                {
                    var buffer = dstManager.GetBuffer<NodeAnchorBuffer>(entity);

                    // Dont add if contains
                    for (var i = 0; i < buffer.Length; i++)
                        if (buffer[i].Node.Equals(anchorEntity))
                            hasEntity = true;
                    if (!hasEntity)
                        buffer.Add(conversionSystem.GetPrimaryEntity(tr));
                }
            }
        }*/


        /*{
            dstManager.AddBuffer<NodeLinkBuffer>(entity);

            // Create Node Links
            foreach (var nodeChain in nodeLinks)
            {
                //Debug.Log("link" + gameObject.name);
                var e = dstManager.CreateEntity();

                var buffer = dstManager.AddBuffer<GraphLink>(e);

                foreach (var tr in nodeChain.myList) buffer.Add(conversionSystem.GetPrimaryEntity(tr));

                dstManager.SetName(e, "Graph Link");

                dstManager.AddComponentData(e, new GraphNode
                {
                    Node = entity
                });
                dstManager.AddComponentData(e, new GraphAnchor
                {
                    Node = conversionSystem.GetPrimaryEntity(nodeChain.AnchorTransform)
                });
            }*/
    }
}

