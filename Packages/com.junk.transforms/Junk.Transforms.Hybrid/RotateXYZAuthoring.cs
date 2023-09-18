using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Junk.Transforms.Hybrid
{
    public class RotateXYZAuthoring: MonoBehaviour
    {
        [FormerlySerializedAs("speedX")] public float SpeedX;
        [FormerlySerializedAs("speedY")] public float SpeedY;
        [FormerlySerializedAs("speedZ")] public float SpeedZ;
    }
    
    public class RotateXYZBaker : Baker<RotateXYZAuthoring>
    {
        public override void Bake(RotateXYZAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            //AddComponent(entity, new PostRotation());
            //AddComponent(entity, new CompositeRotation());
            AddComponent(entity, new PostRotationEulerXYZ());
            AddComponent(entity, new AxisRotation
            {
                SpeedX = authoring.SpeedX,
                SpeedY = authoring.SpeedY,
                SpeedZ = authoring.SpeedZ
            });
        }
    }
}