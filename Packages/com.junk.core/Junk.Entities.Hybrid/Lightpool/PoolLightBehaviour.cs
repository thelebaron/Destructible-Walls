using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Pool;
// ReSharper disable InconsistentNaming

namespace Junk.Entities
{
    /// <summary>
    /// Simple transform copying
    /// </summary>
    public class PoolLightBehaviour : MonoBehaviour
    {
        public Light Light;
        
        private void Start()
        {
            Light = gameObject.AddComponent<Light>();
            Debug.Log("Created a light " + Light);
        }
    }
    
}