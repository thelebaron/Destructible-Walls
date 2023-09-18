using Unity.Entities;

namespace Junk.Entities
{
    /// <summary>
    /// A "dumb" prefab buffer that stores entities marked for prefab creation.
    /// These are only used by queries that need to be serialized.
    /// </summary>
    public struct ScenePrefabData : IBufferElementData
    {
        public Entity Prefab;
    }
}