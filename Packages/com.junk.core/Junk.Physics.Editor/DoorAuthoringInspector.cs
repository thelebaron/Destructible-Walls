using Junk.Physics.Hybrid;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Physics.Authoring;
using UnityEditor;

namespace Junk.Physics.Editor
{
    [CustomEditor(typeof(DoorAuthoring))]
    public class DoorAuthoringInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var doorAuthoring = target as DoorAuthoring;
            
            var hasPhysicsShape = doorAuthoring.GetComponent<PhysicsShapeAuthoring>() != null;
            var hasPhysicsBody  = doorAuthoring.GetComponent<PhysicsBodyAuthoring>() != null;

            if (!hasPhysicsShape)
            {
                if (GUILayout.Button("Add Physics Shape"))
                {
                    doorAuthoring.gameObject.AddComponent<PhysicsShapeAuthoring>();
                    doorAuthoring.SetCollisionFilters(doorAuthoring);
                }
            }

            if (!hasPhysicsBody)
            {
                if(GUILayout.Button("Add Physics Body"))
                {
                    var body = doorAuthoring.gameObject.AddComponent<PhysicsBodyAuthoring>();
                    body.MotionType = BodyMotionType.Kinematic;
                }
            }

            if (hasPhysicsShape)
            {
                if (GUILayout.Button("Set collision filters(BelongsTo/CollidesWith)"))
                {
                    doorAuthoring.SetCollisionFilters(doorAuthoring);
                }
            }
            
            if (GUILayout.Button("Fit collider"))
            {
                if (doorAuthoring != null)
                {
                    var physicsShape = doorAuthoring.GetComponent<PhysicsShapeAuthoring>();
                    Assert.IsNotNull(physicsShape, "PhysicsShapeAuthoring is missing");
                    var physicsBody = doorAuthoring.GetComponent<PhysicsBodyAuthoring>();
                    Assert.IsNotNull(physicsBody, "PhysicsBodyAuthoring is missing");
                
                    if (physicsShape != null && physicsBody != null)
                    {
                        FitColliderShapeToExistingPhysics(doorAuthoring, physicsShape);
                    }
                    if(physicsShape == null && physicsBody != null)
                    {
                        FitColliderShape(doorAuthoring);
                    }
                }
            }
            if (GUILayout.Button("Expand Trigger Size"))
            {
                doorAuthoring.triggerDimensions += Vector3.one * 0.25f;
                // force update
                EditorUtility.SetDirty(this);
            }
            if (GUILayout.Button("Shrink Trigger Size"))
            {
                doorAuthoring.triggerDimensions -= Vector3.one * 0.25f;
                EditorUtility.SetDirty(this);
            }
            
            if (GUILayout.Button("Log Extents(debug importer)"))
            {
                Debug.Log(doorAuthoring.extents);
            }
            
            DrawDefaultInspector();
        }

        private void FitColliderShape(DoorAuthoring authoring)
        {
            if (authoring.GetComponent<MeshFilter>() == null)
            {
                Debug.LogError("Missing a MeshRenderer component and no PhysicsAuthoring components are present, cannot fit collider shape.");
                return;
            }
            var meshFilter = authoring.GetComponent<MeshFilter>();
            var mesh       = meshFilter.sharedMesh;
            var center     = mesh.bounds.center;
            var extents    = new Vector3(mesh.bounds.size.x * authoring.transform.localScale.x,mesh.bounds.size.y * authoring.transform.localScale.y,mesh.bounds.size.z * authoring.transform.localScale.z);
            authoring.colliderShape  = extents;
            authoring.colliderOffset = center;
        }

        private void FitColliderShapeToExistingPhysics(DoorAuthoring authoring, PhysicsShapeAuthoring shapeAuthoring)
        {
            if (shapeAuthoring.ShapeType != ShapeType.Box)
            {
                Debug.LogError("Shape types other than Box are not currently implemented");
                return;
            }
            
            var colliderProperties = shapeAuthoring.GetBoxProperties();
            var center             = colliderProperties.Center;
            var size            = colliderProperties.Size;
            authoring.colliderShape  = size;
            authoring.colliderOffset = center;
            
            authoring.triggerDimensions = size;
            authoring.triggerCenter     = center;
        }
    }
}