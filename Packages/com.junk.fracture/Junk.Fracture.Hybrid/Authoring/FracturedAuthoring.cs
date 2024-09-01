using Unity.Entities.Hybrid.Baking;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    //[RequireComponent(typeof(LinkedEntityGroupAuthoring))]
    public class FracturedAuthoring: MonoBehaviour
    {
        #if UNITY_EDITOR
        public FractureCache FractureCache;
        #endif
    }
}