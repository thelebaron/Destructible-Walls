using System;
using Unity.Mathematics;
using Unity.Transforms;

namespace Junk.Entities.Hybrid
{
    [Serializable]
    public class LocalBakingTransform
    {
        public float3 Position = new(0, 0, 0);
        public float3 Rotation = new(0, 0, 0);
        public float Scale = 1;
        
        public LocalTransform ToLocalTransform()
        {
            return new LocalTransform
            {
                Position = this.Position,
                Rotation = quaternion.EulerXYZ(this.Rotation),
                Scale    = this.Scale
            };
        }
    }
}