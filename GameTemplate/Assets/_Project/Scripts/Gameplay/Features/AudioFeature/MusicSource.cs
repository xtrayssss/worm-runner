using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.AudioFeature
{
    public class MusicSource : AudioSourceBase
    {
        public void ConfigureAndPlay(AudioClip clip, float volume, bool shouldLoop)
        {
            AudioSource.clip = clip;
            AudioSource.volume = volume;
            AudioSource.loop = shouldLoop;
            IsFree = false;
            AudioSource.Play();
        }
    }
}