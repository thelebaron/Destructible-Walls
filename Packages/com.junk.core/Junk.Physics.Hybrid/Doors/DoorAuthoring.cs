using Junk.Entities;
using Unity.Entities.Hybrid.Baking;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Junk.Physics.Hybrid
{
    [DisallowMultipleComponent]
    [SelectionBase]
    [RequireComponent(typeof(LinkedEntityGroupAuthoring))]
    public partial class DoorAuthoring : MonoBehaviour
    {
        public DoorMovementType movementType;
        public float            speed    = 7.5f;
        public float            duration = 0.2f; // how long the door opens or closes for
        public float            lip;
        public Vector3          extents; // renderer extents. this is used to calculate how far to travel when linear motion is used
        
        [Header("Trigger")]
        public PhysicsCategoryTags triggerBelongsTo;
        public PhysicsCategoryTags triggerCollidesWith;
        public Vector3 triggerCenter;
        public Vector3 triggerDimensions = Vector3.one;
    
        [Header("Door")]
        public Vector3 direction;
        public bool3 rotationAxis;
        public bool  negativeDirection;
        
        public bool rotationInOppositeOfTriggerer;
    
        [Header("Collider")]
        public Vector3 colliderShape = Vector3.one;
        public Vector3 colliderOffset = Vector3.zero;
        public PhysicsCategoryTags colliderBelongsTo;
        public PhysicsCategoryTags colliderCollidesWith;
        
        // Scopa related 
        // If this is set to other than a blank entry, another entity should be triggering it, so do not create a trigger.
        public string TargetName;
        
        private bool initialized;
        
        void OnValidate()
        {
            if (!initialized)
            {
                initialized       = true;
                FitColliderShape(this);
                FitTriggerShape(this);
                SetCollisionFilters(this);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(triggerCenter + transform.position, triggerDimensions);
        
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + colliderOffset, colliderShape);
            Gizmos.color = new Color(0f, 0f, 1f, 0.15f);
            Gizmos.DrawCube(transform.position + colliderOffset, colliderShape);
        }

        private static void FitColliderShape(DoorAuthoring authoring)
        {
            MeshFilter meshFilter = null;
            Mesh       mesh      = null;
            if (authoring.GetComponent<MeshFilter>() == null)
            {
                var children = authoring.GetComponentsInChildren<MeshFilter>();
                if (children.Length == 0)
                {
                    Debug.LogError("Missing a MeshRenderer component and no PhysicsAuthoring components are present, cannot fit collider shape.");
                    return;
                }
                
                if (children.Length > 1)
                {
                    Debug.LogWarning("Multiple MeshFilters found, using the first one.");
                }
                meshFilter = children[0];
                
                if(meshFilter == null)
                {
                    Debug.LogError("MeshFilter has no mesh assigned.");
                    return;
                }
                mesh       = meshFilter.sharedMesh;
            }
            
            var center     = meshFilter.sharedMesh.bounds.center;
            var extents    = new Vector3(mesh.bounds.size.x * authoring.transform.localScale.x,mesh.bounds.size.y * authoring.transform.localScale.y,mesh.bounds.size.z * authoring.transform.localScale.z);
            authoring.colliderShape  = extents;
            authoring.colliderOffset = center;
            
            var physicsShape = authoring.GetComponent<PhysicsShapeAuthoring>();
            var physicsBody = authoring.GetComponent<PhysicsBodyAuthoring>();
            
            physicsBody.MotionType = BodyMotionType.Kinematic;

            var shape = extents;
            // make a guess as to the direction of the door movement based on the shape
            if (shape.x > shape.z)
            {
                authoring.rotationAxis = new bool3(true, false, false);
            }
            else
            {
                authoring.rotationAxis = new bool3(false, false, true);
            }
        }

        public static void FitTriggerShape(DoorAuthoring authoring)
        {
            // use existing collider shape and offset
            authoring.triggerDimensions = authoring.colliderShape;
            authoring.triggerCenter     = authoring.colliderOffset;
            
            var physicsShape = authoring.GetComponent<PhysicsShapeAuthoring>();
            authoring.triggerDimensions = physicsShape.GetBoxProperties().Size;
            authoring.triggerCenter     = physicsShape.GetBoxProperties().Center;

            authoring.triggerDimensions.x = 3f;
            authoring.triggerDimensions.z = 3f;
        }


        public void SetCollisionFilters(DoorAuthoring authoring)
        {
            var shape = authoring.gameObject.GetComponent<PhysicsShapeAuthoring>();
            shape.BelongsTo = new PhysicsCategoryTags
            {
                Category00 = false,
                Category01 = false,
                Category02 = true,
                Category03 = false,
                Category04 = false,
                Category05 = false,
                Category06 = false,
                Category07 = false,
                Category08 = false,
                Category09 = false,
                Category10 = false,
                Category11 = false,
                Category12 = false,
                Category13 = false,
                Category14 = false,
                Category15 = false,
                Category16 = false,
                Category17 = false,
                Category18 = false,
                Category19 = false,
                Category20 = false,
                Category21 = false,
                Category22 = false,
                Category23 = false,
                Category24 = false,
                Category25 = false,
                Category26 = false,
                Category27 = false,
                Category28 = false,
                Category29 = false,
                Category30 = false,
                Category31 = false
            };
            shape.CollidesWith = new PhysicsCategoryTags
            {
                Category00 = true,
                Category01 = true,
                Category02 = true,
                Category03 = true,
                Category04 = true,
                Category05 = true,
                Category06 = true,
                Category07 = false,
                Category08 = true,
                Category09 = false,
                Category10 = false,
                Category11 = true,
                Category12 = false,
                Category13 = false,
                Category14 = false,
                Category15 = false,
                Category16 = false,
                Category17 = false,
                Category18 = false,
                Category19 = false,
                Category20 = false,
                Category21 = false,
                Category22 = false,
                Category23 = false,
                Category24 = false,
                Category25 = false,
                Category26 = false,
                Category27 = false,
                Category28 = false,
                Category29 = false,
                Category30 = false,
                Category31 = false
            };
            
            authoring.colliderBelongsTo = new PhysicsCategoryTags
            {
                Category00 = false,
                Category01 = false,
                Category02 = true,
                Category03 = false,
                Category04 = false,
                Category05 = false,
                Category06 = false,
                Category07 = false,
                Category08 = false,
                Category09 = false,
                Category10 = false,
                Category11 = false,
                Category12 = false,
                Category13 = false,
                Category14 = false,
                Category15 = false,
                Category16 = false,
                Category17 = false,
                Category18 = false,
                Category19 = false,
                Category20 = false,
                Category21 = false,
                Category22 = false,
                Category23 = false,
                Category24 = false,
                Category25 = false,
                Category26 = false,
                Category27 = false,
                Category28 = false,
                Category29 = false,
                Category30 = false,
                Category31 = false
            };
            authoring.colliderCollidesWith = new PhysicsCategoryTags
            {
                Category00 = true,
                Category01 = true,
                Category02 = true,
                Category03 = true,
                Category04 = true,
                Category05 = true,
                Category06 = true,
                Category07 = false,
                Category08 = true,
                Category09 = false,
                Category10 = false,
                Category11 = true,
                Category12 = false,
                Category13 = false,
                Category14 = false,
                Category15 = false,
                Category16 = false,
                Category17 = false,
                Category18 = false,
                Category19 = false,
                Category20 = false,
                Category21 = false,
                Category22 = false,
                Category23 = false,
                Category24 = false,
                Category25 = false,
                Category26 = false,
                Category27 = false,
                Category28 = false,
                Category29 = false,
                Category30 = false,
                Category31 = false
            };
            
            authoring.triggerBelongsTo    = Junk.Physics.Hybrid.BakingLayers.DoorTriggerBelongsTo();
            authoring.triggerCollidesWith = Junk.Physics.Hybrid.BakingLayers.DoorTriggerCollidesWith();
        }
    }


}