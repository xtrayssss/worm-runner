using System;
using UnityEngine.Audio;

namespace _Project.Scripts.Gameplay.Features.AudioFeature
{
    [Serializable]
    public class AudioSettings
    {
        public AudioMixerGroup MasterMixerGroup;
        public AudioMixerGroup MusicMixerGroup;
        public AudioMixerGroup SoundEffectsMixerGroup;
        public int SoundSourcePoolSize = 10;
        public int MusicSourcePoolSize = 2;
    }
}