using UnityEngine;

namespace thelebaron.bee
{
    /// <summary>
    /// Pool for audio. Created at runtime.
    /// </summary>
    public class SoundPoolIndex : MonoBehaviour
    {
        public AudioSource AudioSource => m_AudioSource;
        public static int TotalActiveSounds;
        private AudioSource m_AudioSource;
        private Transform m_Transform;
        private bool activeSound;
        
        public void Start()
        {
            m_Transform = transform;

            var audiosource = GetComponent<AudioSource>();
            if (audiosource == null)
            {
                m_AudioSource = gameObject.AddComponent<AudioSource>();
            }
            else
            {
                m_AudioSource = audiosource;
            }
            m_AudioSource.spatialBlend = 1;
            m_AudioSource.playOnAwake = false;
            m_AudioSource.priority = 256;
        }
        
        private void Update()
        {
            if (!m_AudioSource.isPlaying && activeSound)
            {
                TotalActiveSounds--;
                activeSound = false;
            }
        }

        public void Play(AudioClip clip, Vector3 position)
        {
            if(m_AudioSource.isPlaying)
                return;
            
            activeSound = true;
            TotalActiveSounds++;
            m_AudioSource.clip = clip;
            m_AudioSource.Play();
            m_Transform.position = position;
        }
    }
}