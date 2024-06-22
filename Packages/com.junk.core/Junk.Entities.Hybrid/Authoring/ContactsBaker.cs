using Junk.Entities.Hybrid;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    #if UNITY_EDITOR
    /// <summary>
    /// We use the GameSettingsAuthoring as our authoring component.
    /// </summary>
    public class ContactsBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var contactSettingsObject = ContactsSettingsObject.GetOrCreateSettings();
            AddComponent(entity,
                new PrefabEntityData
                {
                    BulletHoleTiny = GetEntity(contactSettingsObject.BulletholeTiny, TransformUsageFlags.Dynamic),
                    BloodSplatTiny = GetEntity(contactSettingsObject.BloodsplatTiny, TransformUsageFlags.Dynamic),
                    DebrisTiny1 = GetEntity(contactSettingsObject.DebrisTiny1, TransformUsageFlags.Dynamic)
                });
        }
    }
    
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(BakingSystemGroup))]
    public partial struct ContactBakingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var prefabEntityData in SystemAPI.Query<PrefabEntityData>())
            {
                ecb.AddComponent<Contact>(prefabEntityData.BulletHoleTiny);
                ecb.AddComponent<Contact>(prefabEntityData.BloodSplatTiny);
                ecb.AddComponent<Contact>(prefabEntityData.DebrisTiny1);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    #endif
}