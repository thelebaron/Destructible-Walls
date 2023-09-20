using System.Linq;
using Junk.Destroy.Authoring;
using Junk.Destroy.Hybrid;
using Project.Scripts.Utils;
using Unity.Entities;
using UnityEngine;
using Joint = UnityEngine.Joint;
using Material = UnityEngine.Material;

namespace Junk.Destroy
{
    [ExecuteAlways,SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class FractureAuthoring : MonoBehaviour
    {
        public FractureCache Cache;
        
        public float              density     = 500;
        public int                totalChunks = 20;
        public int                seed;
        public Material           insideMaterial;
        public Material           outsideMaterial;
        public float              breakForce = 100;

        public FractureWorkingData FractureWorkingData;
        
        private void Joints(GameObject child, float breakForce)
        {
            var rb = child.GetComponent<Rigidbody>();
            var mesh = child.GetComponent<MeshFilter>().sharedMesh;
            var overlaps = mesh.vertices
                .Select(v => child.transform.TransformPoint(v))
                .SelectMany(v => UnityEngine.Physics.OverlapSphere(v, 0.01f))
                .Where(o => o.GetComponent<Rigidbody>())
                .ToSet();

            foreach (var overlap in overlaps)
            { 
                if (overlap.gameObject != child.gameObject)
                {
                    var joint = overlap.gameObject.AddComponent<FixedJoint>();
                    joint.connectedBody = rb;
                    joint.breakForce = breakForce;
                }
            }

            foreach (Transform tr in transform)
            {
                var connectednode = tr.gameObject.GetComponent<NodeAuthoring>();
                if (connectednode == null)
                {
                    var node = tr.gameObject.AddComponent<NodeAuthoring>();
                    node.dirty = true;
                }
                
                // Get all joints and add a node to each child with its joint neighbors
                var joints = tr.GetComponents<Joint>();
                foreach (var joint in joints)
                {
                    var node = joint.transform.GetComponent<NodeAuthoring>();
                    
                    if(!node.connections.Contains(joint.connectedBody.transform))
                        node.connections.Add(joint.connectedBody.transform);
                }
                
                var removeVelocity = tr.gameObject.GetComponent<RemoveVelocity>();
                if(removeVelocity==null)
                    tr.gameObject.AddComponent<RemoveVelocity>();
            }
            
        }
    }

    public class FractureBaker : Baker<FractureAuthoring>
    {
        public override void Bake(FractureAuthoring authoring)
        {
            AddBuffer<ConnectionGraph>(GetEntity(TransformUsageFlags.Dynamic));
        }
    }
}