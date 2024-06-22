using UnityEngine;

namespace Junk.Break.Hybrid
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public class BreakableAuthoring : MonoBehaviour
    {
        public FractureCache FractureCache;
        public GameObject    FracturedObject;
    }
}