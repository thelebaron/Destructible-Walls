using System;
using Unity.Entities;

namespace Junk.Core.Creation
{
    [Serializable]
    public class DiskEntity
    {
        public int Index;
        public int Version;
        
        public Entity ToEntity()
        {
            var entity = new Entity
            {
                Index   = Index,
                Version = Version
            };
            return entity;
        }
        
        public DiskEntity(Entity entity)
        {
            Index   = entity.Index;
            Version = entity.Version;
        }
        
        public static implicit operator Entity(DiskEntity diskEntity)
        {
            return diskEntity.ToEntity();
        }
        
        public static implicit operator DiskEntity(Entity entity)
        {
            return new DiskEntity(entity);
        }
    }
}