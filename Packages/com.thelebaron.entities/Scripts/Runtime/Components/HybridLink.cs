using Unity.Entities;
using UnityEngine;

namespace thelebaron.bee
{
    /// <summary>
    /// Stores a monobehaviour's audiosource and audioclip, for simple audio access within entities.
    /// </summary>
    public class HybridLink : IComponentData
    {
        public AudioSource AudioSource;
        public AudioClip Clip;
    }
}