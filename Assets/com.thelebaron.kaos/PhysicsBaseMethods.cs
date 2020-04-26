using Unity.Entities;
using Unity.Physics;

namespace Destructibles
{
    public static class PhysicsBaseMethods
    {
        public static Entity CreateJoint(BlobAssetReference<JointData> jointData, Entity entityA, Entity entityB, EntityManager entityManager, bool enableCollision = false)
        {
            ComponentType[] componentTypes = new ComponentType[1];
            componentTypes[0] = typeof(PhysicsJoint);
            Entity jointEntity = entityManager.CreateEntity(componentTypes);
            entityManager.SetComponentData(jointEntity, new PhysicsJoint
            {
                JointData       = jointData,
                EntityA         = entityA,
                EntityB         = entityB,
                EnableCollision = enableCollision ? 1 : 0
            });
            return jointEntity;
        }
    }
}