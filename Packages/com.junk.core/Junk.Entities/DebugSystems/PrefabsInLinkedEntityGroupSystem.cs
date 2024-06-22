using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    [DisableAutoCreation]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    public partial struct PrefabsInLinkedEntityGroupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Remove all line prefabs from the linked entity group - otherwise this causes prefabs to be stripped of the prefab component if the main entity is instantiated
            // and all sorts of errors that are hard to diagnose.
            foreach (var (linkedEntityGroup, entity) in SystemAPI.Query<DynamicBuffer<LinkedEntityGroup>>()
                         .WithOptions(EntityQueryOptions.Default | EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabledEntities).WithEntityAccess())
            {
                for (int i = linkedEntityGroup.Length - 1; i >= 0; i--)
                {
                    // Ignore solo entities
                    if(linkedEntityGroup[i].Value == entity)
                        continue;
                    
                    if (SystemAPI.HasComponent<Prefab>(linkedEntityGroup[i].Value))
                    {
                        Debug.Log($"Found a prefab entity: {linkedEntityGroup[i].Value}, in the linked entity group on Entity: {entity}, this can cause incoherent results at runtime.");
                        
                        // get all components of the prefab entity
                        var componentTypes = state.EntityManager.GetComponentTypes(entity);
                        var compList       = "" + entity + " has components: ";
                        foreach (var componentType in componentTypes)
                        {
                            // get Name of the component
                            var componentName = componentType.GetManagedType().Name;
                            compList = compList + componentName + ", ";
                        }
                        componentTypes.Dispose();
                        var prefabList = "" + linkedEntityGroup[i].Value + " has components: ";
                        componentTypes = state.EntityManager.GetComponentTypes(linkedEntityGroup[i].Value);
                        foreach (var componentType in componentTypes)
                        {
                            // get Name of the component
                            var componentName = componentType.GetManagedType().Name;
                            prefabList = prefabList + componentName + ", ";
                        }
                        Debug.Log(compList + "\n" + prefabList);
                    }
                }
            }
        }
    }
}