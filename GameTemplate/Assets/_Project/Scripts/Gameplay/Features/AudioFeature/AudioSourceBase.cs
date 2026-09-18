using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Scripts.Gameplay.Features.AudioFeature
{
    public abstract class AudioSourceBase : MonoBehaviour
    {
        protected AudioSource AudioSource;
        public bool IsFree { get; set; } = true;
        
        public AudioSource GetAudioSource() => AudioSource;

        public void Construct(AudioMixerGroup mixerGroup)
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.spatialBlend = 0f;
            AudioSource.outputAudioMixerGroup = mixerGroup;
        }

        public void SetMuted(bool isMuted) => AudioSource.mute = isMuted;
        public void Stop() => AudioSource.Stop();
        public void Pause() => AudioSource.Pause();
        public void Resume() => AudioSource.UnPause();
    }
}