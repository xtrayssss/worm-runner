using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record RiflemanEnemyConfig : BaseInteractiveObjectConfig<RiflemanEnemyConfig>
    {
        [SerializeField]
        private float _shootDamage;

        public float ShootDamage => BaseConfig._shootDamage;
    }
}