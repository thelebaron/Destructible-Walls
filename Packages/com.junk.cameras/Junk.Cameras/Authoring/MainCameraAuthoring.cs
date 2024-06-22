using System;
using Junk.Transforms;
using Junk.Transforms.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Cameras
{

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class MainCameraAuthoring : MonoBehaviour
    {
        public TransformType transformType;
        
        /// <summary>
        /// Note - baker does not appear to work in a build so we have to do this manually in Update
        /// </summary>
        public void Update()
        {
            // Manual camera conversion
            if(World.DefaultGameObjectInjectionWorld== null)
                return;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entity = entityManager.CreateEntity();
            entityManager.AddComponentData(entity, new LocalToWorld{Value = float4x4.identity});
            entityManager.AddComponentData(entity, LocalTransform.FromPositionRotation(transform.position, transform.rotation));
                
            entityManager.AddComponent<MainCameraData>(entity);
            entityManager.AddComponentObject(entity, GetComponent<Camera>());
                
            /*AddComponentObject(entity, new ManagedTransform
            {
                Transform = authoring.transform
            });*/
            entityManager.AddComponentData(entity, new HybridTransform
            {
                Options = transformType
            });
                
            entityManager.AddComponent<RotationEulerXYZ>(entity);
            
            const float k_CursorIconSize = 64;
            entityManager.AddComponentData(entity, new SceneCamera
            {
                MouseCursorRect  = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize),
                ScreenCenterRect = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize)
            });

            entityManager.SetComponentEnabled<SceneCamera>(entity, false);
            
            this.enabled = false;
        }
        
        public class MainCameraAuthoringBaker : Baker<MainCameraAuthoring>
        {
            public override void Bake(MainCameraAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent<MainCameraData>(entity);
                
                /*AddComponentObject(entity, new ManagedTransform
                {
                    Transform = authoring.transform
                });*/
                AddComponent(entity, new HybridTransform
                {
                    Options = TransformType.CopyToGameObject
                });
                
                AddComponent<RotationEulerXYZ>(entity);
                const float k_CursorIconSize = 64;
                AddComponent(entity, new SceneCamera
                {
                    MouseCursorRect  = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize),
                    ScreenCenterRect = new Rect(0, 0, k_CursorIconSize, k_CursorIconSize)
                });

                SetComponentEnabled<SceneCamera>(entity, false);
            }
        }
    }

    public struct MainCameraComponentData : IComponentData
    {
    }
}