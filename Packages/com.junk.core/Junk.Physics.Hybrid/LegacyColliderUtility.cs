
#if UNITY_EDITOR

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Collider = Unity.Physics.Collider;
using LegacyBoxCollider = UnityEngine.BoxCollider;
using LegacySphereCollider = UnityEngine.SphereCollider;
using LegacyCapsuleCollider = UnityEngine.CapsuleCollider;

namespace Junk.Physics.Hybrid
{
    public static class LegacyRigidbodyUtility
    {
        /// <summary>
        /// Creates a physics mass from a legacy rigidbody.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static PhysicsState CreatePhysicsState(UnityEngine.Component component)
        {
            var unitSphere    = MassProperties.UnitSphere;
            var rigid         = component.GetComponent<UnityEngine.Rigidbody>();
            var kinematicMass = PhysicsMass.CreateKinematic(unitSphere);
            var dynamicMass   = PhysicsMass.CreateDynamic(unitSphere, rigid.mass);
            return new PhysicsState(kinematicMass, dynamicMass, CollisionFilter.Default, CollisionFilter.Default);
            
        }
    }
    
    public static class LegacyColliderUtility
    {
        public static PhysicsCollider CreateCollider(UnityEngine.Component component)
        {
            var collider = component.GetComponent<UnityEngine.Collider>();
            
            if (collider.GetType() == typeof(UnityEngine.BoxCollider))
            {
                var blob        = CreateBoxCollider((UnityEngine.BoxCollider)collider);
                return new PhysicsCollider { Value = blob };
            }
            if (collider.GetType() == typeof(UnityEngine.SphereCollider))
            {
                var blob        = CreateSphereCollider((UnityEngine.SphereCollider)collider);
                return new PhysicsCollider { Value = blob };
            }
            if (collider.GetType() == typeof(UnityEngine.CapsuleCollider))
            {
                var blob        = CreateCapsuleCollider((UnityEngine.CapsuleCollider)collider);
                return new PhysicsCollider { Value = blob };
            }

            Debug.LogError("Case not handled: Should not be here.");
            return default;
        }


        private static BlobAssetReference<Collider> CreateBoxCollider(UnityEngine.BoxCollider collider)
        {
            return Unity.Physics.BoxCollider.Create(new BoxGeometry
            {
                BevelRadius = 0.001f,
                Center      = collider.center,
                Orientation = quaternion.identity,
                Size        = collider.size
            });
        }

        private static BlobAssetReference<Collider> CreateSphereCollider(UnityEngine.SphereCollider collider)
        {
            return Unity.Physics.SphereCollider.Create(new SphereGeometry
            {
                Center = collider.center,
                Radius = collider.radius
            });
        }

        private static BlobAssetReference<Collider> CreateCapsuleCollider(UnityEngine.CapsuleCollider collider)
        {
            var shape = collider;
            var height = collider.height / 2;
            var axis = new float3 { [ shape.direction ] = 1f };
            
            //Debug.Log(axis);
            var center = (float3)shape.center;
            
            return Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry
            {
                Radius = collider.radius,
                Vertex0 = axis * -height/2 + center,
                Vertex1 = axis * height/2 + center,
            });
            
            //system.DstEntityManager.AddComponentData(entity, new PhysicsCollider {Value = blob});
        } 
    }
}

#endif