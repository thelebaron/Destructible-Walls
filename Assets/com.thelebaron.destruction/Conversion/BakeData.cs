using System;
using UnityEngine;

namespace thelebaron.Destruction.Authoring
{
    /// <summary> Object data that gets passed around during mesh conversion </summary>
    [Serializable]
    public class BakeData
    {
        public GameObject    gameObject;
        public string        name = "new Destructible";
        public int           seed = 3;
        public System.Random random;
        public float         density     = 500;
        public int           totalChunks = 20;
        public Mesh          mesh;
        public Material      insideMaterial;
        public Material      outsideMaterial;
        public float         jointBreakForce = 100;
        public float         totalMass;

        public NodeAuthoring[] nodes;
    }
}