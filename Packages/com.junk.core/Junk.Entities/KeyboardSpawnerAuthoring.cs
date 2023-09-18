using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Junk.Entities
{
    public class KeyboardSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        
    }
    
    public class KeyboardSpawnerBaker : Baker<KeyboardSpawnerAuthoring>
    {
        public override void Bake(KeyboardSpawnerAuthoring authoring)
        {
            AddComponent( new SimpleSpawner
            {
                Prefab = GetEntity(authoring.Prefab)
            });
        }
    }

    public struct SimpleSpawner : IComponentData
    {
        public Entity Prefab;
    }

    [DisableAutoCreation]
    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    public partial class KeyboardSpawner : SystemBase
    {
        private float timer;

        protected override void OnUpdate()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            timer -= SystemAPI.Time.DeltaTime;

            var spaceKeyDown = keyboard.spaceKey.isPressed;
            if (spaceKeyDown)
            {
                Debug.Log("temp disable lightning spawner");
                return;
            }
            
            if (spaceKeyDown && timer <= 0)
            {
                timer = 0.2f;
                //EditorApplication.isPaused = true;

                Entities.ForEach((Entity entity, int entityInQueryIndex, ref SimpleSpawner spawner, in LocalToWorld localToWorld) =>
                {
                    var instance = EntityManager.Instantiate(spawner.Prefab);
                    EntityManager.SetName(instance, "lightning spawned");
                    // set localtoworld
                    EntityManager.SetComponentData(instance, new LocalToWorld
                    {
                        Value = localToWorld.Value
                    });
                    EntityManager.SetComponentData(instance, LocalTransform.FromMatrix(localToWorld.Value));
                    
                    EntityManager.SetComponentData(instance, new Rng
                    {
                        Value = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, uint.MaxValue))
                    });
                }).WithStructuralChanges().WithoutBurst().Run();
            }
        }
    }
}