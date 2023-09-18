using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Junk.Core.Creation
{
    [Serializable]
    public partial struct Map
    {
        public List<Shape> shapes;
        public List<Node> nodes;
        public List<Entity> entities;
    }
}