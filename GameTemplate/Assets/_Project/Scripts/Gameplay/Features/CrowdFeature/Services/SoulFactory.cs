using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Services
{
    public sealed class SoulFactory : IService
    {
        private readonly ConfigsService _configsService;

        public SoulFactory(ConfigsService configsService) =>
            _configsService = configsService;

        public SoulView CreateSoul(Vector3 position, SoulType soulType)
        {
            SoulView soulPrefab = _configsService.GetSoulPrefab();

            SoulView soulView = Object.Instantiate(soulPrefab, position + Vector3.up * 0.5f, Quaternion.identity);

            soulView.Construct(GetSoulColor(soulType));

            return soulView;
        }

        private Color GetSoulColor(SoulType soulType)
        {
            return soulType switch
            {
                SoulType.FRIENDLY => Color.white,
                SoulType.ENEMY => new Color(0.6f, 0.6f, 0.6f),
                _ => Color.white
            };
        }
    }
}