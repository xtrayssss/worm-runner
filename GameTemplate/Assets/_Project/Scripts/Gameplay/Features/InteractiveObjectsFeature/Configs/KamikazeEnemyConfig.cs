using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record KamikazeEnemyConfig : BaseInteractiveObjectConfig<KamikazeEnemyConfig>
    {
        [SerializeField]
        private float _explosionRadius = 5f;

        [SerializeField]
        private float _explosionDamage = -5f;

        public float ExplosionRadius => BaseConfig._explosionRadius;
        public float ExplosionDamage => BaseConfig._explosionDamage;
    }
}