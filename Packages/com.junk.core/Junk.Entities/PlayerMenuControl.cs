using Unity.Entities;

namespace Junk.Entities
{
    /// <summary>
    /// For now this is the player control tag, if disabled it means we are in a menu or something (not to assume control of a player controllable entity)
    /// This is controlled by the menu system so setting this manually outside of that will be overriden.
    /// </summary>
    public struct PlayerMenuControl : IComponentData, IEnableableComponent
    {
        
    }
}