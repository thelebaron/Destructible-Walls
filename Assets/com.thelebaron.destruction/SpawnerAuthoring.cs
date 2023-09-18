using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
//todo add baker
public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    private Entity m_Entity;
    private Entity m_PrefabEntity;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            em.Instantiate(m_PrefabEntity);
        }
    }

    public class SpawnerAuthoringBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnerComponentData { Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic) });
        }
    }
}

public struct SpawnerComponentData : IComponentData
{
    public Entity Prefab;
}


