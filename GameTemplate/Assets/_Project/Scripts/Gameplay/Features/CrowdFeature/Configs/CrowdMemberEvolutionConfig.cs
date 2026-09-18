using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Configs
{
    [Serializable]
    [InlineProperty]
    public class CrowdMemberEvolutionConfig
    {
        [PreviewField(80, ObjectFieldAlignment.Center)]
        [SerializeField, AssetsOnly]
        private Sprite _sprite;

        [SerializeField]
        private float _damagePercent;

        [SerializeField]
        private float _cooldownPercent;
        
        [SerializeField]
        private float _healthPercent;

        public Sprite Sprite => _sprite;

        public bool HasSpriteChange => _sprite != null;

        public float GetStatPercent(StatId statId)
        {
            return statId switch
            {
                StatId.DAMAGE => _damagePercent,
                StatId.COOLDOWN => _cooldownPercent,
                StatId.HEALTH => _healthPercent,
                _ => 0f
            };
        }
    }
}