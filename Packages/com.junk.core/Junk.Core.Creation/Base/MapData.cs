using System;
using Unity.Entities;
using Unity.Transforms;

namespace Junk.Core.Creation
{
    public partial struct Map
    {
        [Serializable]
        public struct ID
        {
            public int    index;
            public int    version;

            public static implicit operator Entity(ID e)
            {
                return new Entity
                {
                    Index   = e.index,
                    Version = e.version
                };
            }

            public static implicit operator ID(Entity e)
            {
                return new ID
                {
                    index   = e.Index,
                    version = e.Version
                };
            }
        }
        
        [Serializable]
        public struct Node
        {
            public ID id;
            public TransformData transformData;
        
        }
    
        [Serializable]
        public struct TransformData
        {
            public LocalTransform LocalTransform;
            public LocalToWorld   LocalToWorld;

            public TransformData(LocalTransform lt, LocalToWorld l)
            {
                LocalTransform = lt;
                LocalToWorld = l;
            }
        }
    }
}