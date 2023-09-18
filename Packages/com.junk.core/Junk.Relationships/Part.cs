using System;
using Unity.Entities;

namespace Junk.Relationships
{
    /// <summary>
    /// The value represents the controlling entity of a multi entity relationship. Relationships are not necessarily hierarchical.
    /// </summary>
    //[WriteGroup(typeof(Root))]
    [Serializable]
    public struct Part : IComponentData
    {
        public Entity RootEntity;
    }

}