using Unity.Entities;

namespace Junk.Entities
{
    public struct GameSave : IComponentData
    {
        public DataState DataState;
    }
    
    public enum DataState
    {
        None,
        Loading,
        Saving,
    }
}