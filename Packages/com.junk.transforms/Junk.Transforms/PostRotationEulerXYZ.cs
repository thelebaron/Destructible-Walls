using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Transforms
{
    public struct PostRotation : IComponentData
    {
        public quaternion Value;
    }
    
    public struct RotationEulerXYZ : IComponentData
    {
        public float3 Value;
    }
    
    public struct PostRotationEulerXYZ : IComponentData
    {
        public float3 Value;
    }
    
    public struct CompositeRotation : IComponentData
    {
        public float4x4 Value;
    }
    
    /*
    [Serializable]
    [WriteGroup(typeof(LocalToWorld))]
    [WriteGroup(typeof(LocalToParent))]
    public struct CompositeRotation : IComponentData
    {
        public float4x4 Value;
    }

    [Serializable]
    [WriteGroup(typeof(CompositeRotation))]
    public struct PostRotation : IComponentData
    {
        public quaternion Value;
    }

    [Serializable]
    [WriteGroup(typeof(CompositeRotation))]
    public struct RotationPivot : IComponentData
    {
        public float3 Value;
    }

    [Serializable]
    [WriteGroup(typeof(CompositeRotation))]
    public struct RotationPivotTranslation : IComponentData
    {
        public float3 Value;
    }*/
}