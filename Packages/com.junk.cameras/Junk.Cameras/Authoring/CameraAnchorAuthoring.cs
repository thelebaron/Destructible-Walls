using Junk.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Junk.Cameras
{
    // Fixed anchor for the camera to follow
    // This is a rigid hierachy connection
    public class CameraAnchorAuthoring : MonoBehaviour
    {
        
    }

    public class CameraAnchorBaker : Baker<CameraAnchorAuthoring>
    {
        public override void Bake(CameraAnchorAuthoring authoring)
        {
            AddComponent<CameraAnchor>(GetEntity(TransformUsageFlags.Dynamic));
        }
    }

    /// <summary>
    /// Cant remember but I think this should find a scene camera and attach it to the player
    /// Creates a hybrid camera entity.
    /// Adds transform components to the camera entity.
    /// Adds managed camera transform components to the camera entity.
    /// Parents hybrid camera entity to player camera entity
    /// Removes CameraAnchor component from player camera entity
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CameraAnchorSystem : ISystem
    {
        private EntityQuery query;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCamera>();
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<CameraAnchor>();
            builder.WithOptions(EntityQueryOptions.IncludePrefab);
            query = state.GetEntityQuery(builder);
        }
    
        public void OnUpdate(ref SystemState state)
        {
            var cameraEntity = SystemAPI.GetSingletonEntity<MainCamera>();
            var ecb                    = new EntityCommandBuffer(Allocator.Temp);
        
            var entityCount = query.CalculateEntityCount();
            if(entityCount == 0) 
                return;

            var entity      = query.GetSingletonEntity();
            var sceneCamera = UnityEngine.Object.FindObjectOfType(typeof(Camera)) as Camera;

            if (sceneCamera == null)
            {
                Debug.LogError("camera is null");
                return;
            }

            if (state.EntityManager.HasComponent<Child>(entity))
            {
                var cameraChildBuffer = state.EntityManager.GetBuffer<Child>(entity);
                cameraChildBuffer.Add(new Child {Value = cameraEntity});
            }
            else
            {
                var cameraChildBuffer = state.EntityManager.AddBuffer<Child>(entity);
                cameraChildBuffer.Add(new Child {Value = cameraEntity});
            }
            
            state.EntityManager.AddComponentData(cameraEntity, new Parent{Value = entity});
            state.EntityManager.AddComponentObject(cameraEntity, sceneCamera);

            // should move this to conversion
            sceneCamera.gameObject.tag = "MainCamera";
            sceneCamera.fieldOfView    = 75f;
            sceneCamera.nearClipPlane  = 0.01f;
            //sceneCamera.farClipPlane = 5000f;
            
            state.EntityManager.RemoveComponent<CameraAnchor>(entity);
        
        }
    }
}