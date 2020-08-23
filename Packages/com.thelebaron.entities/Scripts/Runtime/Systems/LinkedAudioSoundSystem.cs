using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Profiling;

namespace thelebaron.bee
{
    /// <summary>
    /// The Linked Audio Sound System is a system that plays a "linked" sound from an entity component.
    /// The SoundLink can be used from jobs, to play or stop a sound.
    /// The HybridLink is used on the main thread to play an audioclip on its audiosource component.
    ///
    /// During conversion, you must create a gameobject, with an audiosource and feed the hybrid link
    /// with it and the desired audioclip.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class LinkedAudioSoundSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Profiler.BeginSample("LinkedAudioSoundSystem.Update");
            
            // Entities Codegen
            Entities
            .WithName("LinkedAudioSoundSystemJob")
            .ForEach((Entity entity, HybridLink hybridLink, ref SoundLink soundLink, ref LocalToWorld localToWorld) =>
            {
                if (soundLink.Play)
                {
                    hybridLink.AudioSource.transform.position = localToWorld.Position;
                    hybridLink.AudioSource.Play();
                    soundLink.Play = false;
                }
                
                if (soundLink.Stop)
                {
                    hybridLink.AudioSource.Stop();
                    soundLink.Stop = false;
                }

            }).WithoutBurst().Run();
            
            Profiler.EndSample();
        }
    }
}