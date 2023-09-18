/*
 Obsolete but kept for reference as how to bootstrap prior to scene loading
 using System;
using Junk.Math;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Junk.Cameras
{
    /// <summary>
    /// Creates an entity for the scene camera gameObject
    /// </summary>
    internal class DefaultCameraInitializationProxy : MonoBehaviour
    {
        public void Update()
        {
            // Note using Camera.main may not always work as it requires the camera to be tagged as MainCamera, so we use FindObjectOfType<Camera>()
            var sceneCamera = Object.FindObjectOfType(typeof(Camera)) as Camera;
            if (sceneCamera == null)
                return;
            
            // If worlds are not created yet, we can't create entities
            if(World.All.Count < 1)
                return;
            
            // If the default gameobject world is not created yet return
            if(World.DefaultGameObjectInjectionWorld == null)
                return;

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

#if UNITY_EDITOR
            var query = entityManager.CreateEntityQuery(typeof(MainCamera));
            if (query.CalculateEntityCount() > 0)
            {
                throw new InvalidOperationException("MainCamera entity already exists");
            }
#endif

            var entity = entityManager.CreateEntity();
            entityManager.AddComponent<MainCamera>(entity);
            entityManager.AddComponentData<LocalToWorld>(entity, new LocalToWorld{Value = float4x4.TRS(Vector3.up * 10, quaternion.identity, Vector3.one)});
            entityManager.AddComponent<LocalTransform>(entity);
#if UNITY_EDITOR
            entityManager.SetName(entity, "# MainCamera");
#endif
            
            entityManager.AddComponentObject(entity, new CameraReference
            {
                Camera = sceneCamera
            });
            
            sceneCamera.name    = "# MainCamera";

            Destroy(gameObject);
        }
    }
    
    // notes: make runtime system to handle conversion
    // create archetype for camera
    // add camera component to archetype
    // turn camerareference into state component
}*/