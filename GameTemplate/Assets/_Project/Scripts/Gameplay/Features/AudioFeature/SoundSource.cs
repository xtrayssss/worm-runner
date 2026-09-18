using _Project.Scripts.Gameplay.Features.LoggerFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.AudioFeature
{
    public sealed class SoundSource : AudioSourceBase
    {
        public void ConfigureAndPlay(AudioClip clip, float volume)
        {
            AudioSource.clip = clip;
            AudioSource.volume = volume;
            AudioSource.loop = false;
            IsFree = false;
            AudioSource.Play();

            L.Log($"AudioSource: {AudioSource.clip.name} is playing");
        }

        private void Update()
        {
            if (!AudioSource.isPlaying)
            {
                IsFree = true;
            }
        }
    }
}