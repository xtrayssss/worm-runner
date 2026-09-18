using _Project.Scripts.Gameplay.Features.LoggerFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.AudioFeature
{
    public sealed class SoundSource3D : AudioSourceBase
    {
        public void ConfigureAndPlay(AudioClip clip, float volume, Vector3 position, float minDistance, float maxDistance)
        {
            AudioSource.clip = clip;
            AudioSource.volume = volume;
            AudioSource.loop = false;
            AudioSource.spatialBlend = 1f;
            AudioSource.rolloffMode = AudioRolloffMode.Linear;
            AudioSource.minDistance = minDistance;
            AudioSource.maxDistance = 70f;
            AudioSource.spread = 180f;
            
            transform.position = position;
            
            IsFree = false;
            AudioSource.Play();

            L.Log($"3D AudioSource: {AudioSource.clip.name} is playing at position {position}");
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