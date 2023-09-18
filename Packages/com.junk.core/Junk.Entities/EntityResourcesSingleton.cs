using Unity.Entities;

namespace Junk.Entities
{
    public struct EntityResourcesSingleton : IComponentData
    {
        public Entity DebrisPebble;
        public Entity DecalBloodSplatter;
        public Entity BloodSpraySpawnerLarge;
    }

}