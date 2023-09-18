using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Transforms.Hybrid
{
    public enum TransformType 
    { 
        CopyToGameObject,
        CopyToEntity
    }
    
    /// <summary>
    /// Component that represents a transform in the world.
    /// new in 0.50.0
    /// For now we always assume this is root space/ not used inside of a hierarchy.
    /// </summary>
    [Serializable]
    public class ManagedTransform : IComponentData
    {
        public Transform     Transform;
        
        public enum TransformComponentOptions
        {
            Translation,
            Rotation,
            Scale,
            All
        }
    }

    /// <summary>
    /// So behaviour can be controlled from a job
    /// </summary>
    public struct HybridTransform : IComponentData
    {
        public TransformType Options;
    }

    public struct TransformPrediction : IComponentData
    {
        public float3 PreviousPosition;
        public float3 Difference;
    }

    // Requires dependency to Junk.Entities
    //[UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    [UpdateInGroup(typeof(StructuralChangePresentationSystemGroup))]
    public partial class HybridTransformSystem : SystemBase
    {
        [BurstCompile]
        public partial struct PredictTransformJob : IJobEntity
        {
            private void Execute(Entity entity, ref LocalToWorld localToWorld, ref LocalTransform localTransform, ref TransformPrediction prediction)
            {
                var difference = localToWorld.Position - prediction.PreviousPosition;
                prediction.PreviousPosition =  localToWorld.Position;
                prediction.Difference       =  difference;
                localTransform.Position     += difference;
            }
        }
        
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ManagedTransform hybrid, ref LocalToWorld localToWorld, ref LocalTransform localTransform, in HybridTransform hybridTransform) =>
            {
                switch (hybridTransform.Options)
                {
                    case TransformType.CopyToGameObject:
                    {
                        hybrid.Transform.position = localToWorld.Position;
                        hybrid.Transform.rotation = localToWorld.Rotation;
                    }
                    break;
                    
                    case TransformType.CopyToEntity:
                    {
                        var pos = hybrid.Transform.position;
                        var rot = hybrid.Transform.rotation;
                        var scale = hybrid.Transform.localScale;
                        Debug.LogWarning("HybridTransformSystem: nonuniform scale not yet implemented");
                        /*
                        if(hasNonUniformScale)
                        {   
                            SetComponent(entity, new NonUniformScale { Value = scale });
                        }*/
                        
                        localToWorld = new LocalToWorld
                        {
                            Value = float4x4.TRS(
                                pos,
                                rot,
                                scale)
                        };
                        localTransform = LocalTransform.FromMatrix(localToWorld.Value);
                    }
                        break;
                }
            }).WithoutBurst().Run();
            
            Dependency = new PredictTransformJob().Schedule(Dependency);
        }
    }
}