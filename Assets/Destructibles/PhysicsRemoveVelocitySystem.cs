using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Destructibles
{
    /// <summary>
    /// This System tells the conversion system to remove any PhysicsVelocity during conversion.
    /// Uses the RemoveVelocity component.
    /// </summary>
    [UpdateAfter(typeof(LegacyBoxColliderConversionSystem))]
    [UpdateAfter(typeof(LegacyCapsuleColliderConversionSystem))]
    [UpdateAfter(typeof(LegacySphereColliderConversionSystem))]
    [UpdateAfter(typeof(LegacyMeshColliderConversionSystem))]
    [UpdateAfter(typeof(PhysicsShapeConversionSystem))]
    [UpdateAfter(typeof(LegacyRigidbodyConversionSystem))]
    public class PhysicsRemoveVelocitySystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach(
                (RemoveVelocity removeVelocity) =>
                {
                    var entity = GetPrimaryEntity(removeVelocity.gameObject);
                    DstEntityManager.RemoveComponent<PhysicsVelocity>(entity);
                }
            );
        }
    }
    
    [DisallowMultipleComponent]
    public class RemoveVelocity : MonoBehaviour
    {
        // Intentionally blank
    }
}