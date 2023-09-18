using Unity.Entities;
using UnityEngine;

namespace Junk.Entities
{
    [DisallowMultipleComponent]
    public class UnmanagedInput : MonoBehaviour
    {
        
    }
    
    public class UnmanagedInputBaker : Baker<UnmanagedInput>
    {
        public override void Bake(UnmanagedInput authoring)
        {

        }
    }
}