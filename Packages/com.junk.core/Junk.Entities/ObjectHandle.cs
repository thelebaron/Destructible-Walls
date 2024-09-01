using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    public class ObjectHandle : Object
    {
        public Entity Entity;

        public ObjectHandle(Entity entity)
        {
            Entity = entity;
        }
    }
}