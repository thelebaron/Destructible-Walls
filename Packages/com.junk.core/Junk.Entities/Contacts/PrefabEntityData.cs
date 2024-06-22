using Unity.Entities;

namespace Junk.Entities
{
    public struct PrefabEntityData : IComponentData
    {
        // Impact decals
        public Entity BulletHoleTiny;
        // Blood decals
        public Entity BloodSplatTiny;
        public Entity DebrisTiny1;
    }

    public struct Contact : IComponentData
    {
        
    }
    
    public struct PhysicsContact : IComponentData
    {
        
    }
}