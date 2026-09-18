using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record ExplosiveBarrelConfig : BaseInteractiveObjectConfig<ExplosiveBarrelConfig>
    {
        [SerializeField]
        private float _explosionRadius = 5f;

        [SerializeField]
        private float _explosionDamage = 20f;

        public float ExplosionRadius => BaseConfig._explosionRadius;
        public float ExplosionDamage => BaseConfig._explosionDamage;
    }
}