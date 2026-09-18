using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.ConfettiFeature.Configs;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ConfettiFeature.Services
{
    public sealed class ConfettiService : IService
    {
        private readonly Dictionary<ConfettiId, ConfettiConfig> _confettiConfigs;
        private readonly AudioService _audioService;

        public ConfettiService(Dictionary<ConfettiId, ConfettiConfig> confettiConfigs, AudioService audioService)
        {
            _confettiConfigs = confettiConfigs;
            _audioService = audioService;
        }

        public void PlayConfetti(ConfettiId confettiId, Vector3 position, Transform parent)
        {
            ConfettiConfig config = _confettiConfigs[confettiId];
            ParticleSystem confettiPrefab = config.Prefab;

            _audioService.PlayRandomSoundVariation(AudioId.Sfx.UI.CONFETTI_EXPLOSION, isSpecial: true);

            Object.Instantiate(confettiPrefab, position, Quaternion.identity, parent);
        }
    }
}