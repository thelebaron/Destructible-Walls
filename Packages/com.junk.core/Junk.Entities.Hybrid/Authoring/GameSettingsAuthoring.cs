using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    //[Icon(k_IconPath)]
    [AddComponentMenu("Tools/Game Settings")]
    public class GameSettingsAuthoring : MonoBehaviour
    {
        private const string k_IconPath = "";
    }
    
    
#if UNITY_EDITOR
    public class GameSettingsBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            var settings = GameSettingsObject.GetOrCreateSettings();
            DependsOn(settings);
            var entity = GetEntity(TransformUsageFlags.None);
                
            var stringtext = "";
            if(settings.SomeString!=null)
                stringtext = settings.SomeString;
                
            AddComponent(entity, new GameSettings
            {
                Volume     = settings.Volume,
                Number     = settings.Number,
                SomeString = new FixedString128Bytes(stringtext)
            });
        }
    }
#endif
}

