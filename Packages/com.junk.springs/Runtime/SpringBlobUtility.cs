using System;
using Junk.Math;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Junk.Springs
{
    public static class SpringBlobUtility
    {
        // see brian will blob video https://www.youtube.com/watch?v=q1_b--k3fQ8&t=680s
        public static BlobAssetReference<SpringLimits> GetSpringBlobData(float minVelocity = 0.0000001f)
        {
            using (var builder = new BlobBuilder(Allocator.TempJob))
            {
                // Construct the root object for the blob asset. Notice the use of `ref`.
                ref var data = ref builder.ConstructRoot<SpringLimits>();

                data.MaxVelocity = 10000.0f;
                data.MinVelocity = minVelocity;
                data.MaxState    = new float3(10000,  10000,  10000);
                data.MinState    = new float3(-10000, -10000, -10000);
                
                // Now copy the data from the builder into its final place, which will be a BlobAssetReference
                var blobAssetReference = builder.CreateBlobAssetReference<SpringLimits>(Allocator.Persistent);
                
                return blobAssetReference;
            }
        }
        
        /// <summary>
        /// Always use this for spring creation(otherwise you need to manually create the softforce buffer,
        /// if one is not added the spring system will not pick up the spring),
        /// returns a Spring Entity, with inputs for the default local rest position, and automatically adds a SoftForce buffer
        /// </summary>
        public static Entity CreateDefaultSpring<T>(this Baker<T> baker, string name, /*ref BlobAssetReference<SpringLimits> blobAssetReference,*/ float3 restState = default) where T : Component
        {
            var additionalEntity = baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name);
            
            var minVelocity     = 0.0000001f; 
            var springComponent = new Spring(restState, minVelocity);
            
            // I think this fixes the persistent but not 100% null blob errors, this needed to be added prior to adding the component?
            baker.AddBlobAsset<SpringLimits>(ref springComponent.SpringLimitsReference, out var blobhash);
            baker.AddComponent(additionalEntity, springComponent);
            var softForceBuffer = baker.AddBuffer<SoftForce>(additionalEntity);
            for (var i = 0; i < 120; i++)
                softForceBuffer.Add(new SoftForce());
            
            return additionalEntity;
        }
        
        
        /// <summary>
        /// Always use this for spring creation(otherwise you need to manually create the softforce buffer,
        /// if one is not added the spring system will not pick up the spring),
        /// returns a Spring Entity, with inputs for the default local rest position, and automatically adds a SoftForce buffer
        /// </summary>
        public static Entity CreateSpring<T>(this Baker<T> baker, string name, float3 stiffness, float3 damping, float3 restState = default) where T : Component
        {
            var additionalEntity = baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name);
            
            var minVelocity     = 0.0000001f; 
            var springComponent = new Spring(restState, minVelocity);
            
            springComponent.Stiffness = stiffness * maths.one;
            springComponent.Damping = damping * maths.one;
            baker.AddBlobAsset<SpringLimits>(ref springComponent.SpringLimitsReference, out var blobhash);
            baker.AddComponent(additionalEntity, springComponent);
            var softForceBuffer = baker.AddBuffer<SoftForce>(additionalEntity);
            for (var i = 0; i < 120; i++)
                softForceBuffer.Add(new SoftForce());
            
            return additionalEntity;
        }
    }
}