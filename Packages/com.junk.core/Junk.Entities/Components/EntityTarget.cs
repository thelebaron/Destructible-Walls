using Unity.Entities;

namespace Junk.Entities
{
    /// <summary>
    /// https://quakewiki.org/wiki/func_door
    ///
    /// If a MapComponent has an EntityTarget definition that is not null/greater than zero length,
    /// it is considered a trigger entity that enables logic on its EntityTarget entity.
    /// </summary>
    public struct EntityTarget : IComponentData
    {
        public Entity Value;
    }
}