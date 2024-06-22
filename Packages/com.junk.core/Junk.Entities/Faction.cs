using System;
using Unity.Entities;

namespace Junk.Entities
{
    /// <summary>
    /// Simple faction id
    /// </summary>
    [Serializable]
    public struct Faction : IComponentData
    {
        public        byte    Value;

        public static Faction GetDefault()
        {
            return new Faction();
        }
    }
}