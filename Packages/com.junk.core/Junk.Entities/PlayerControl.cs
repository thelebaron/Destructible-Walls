using Unity.Entities;

namespace Junk.Entities
{
    /// <summary>
    /// For now this is the player control tag, if disabled it means we are in a menu or something (not to assume control of a player controllable entity)
    /// </summary>
    public struct PlayerControl : IComponentData, IEnableableComponent
    {
        
    }
}