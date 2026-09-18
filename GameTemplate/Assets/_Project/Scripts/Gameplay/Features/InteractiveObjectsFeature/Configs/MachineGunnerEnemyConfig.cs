using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record MachineGunnerEnemyConfig : BaseInteractiveObjectConfig<MachineGunnerEnemyConfig>
    {
        [SerializeField]
        private float _shootDamage = 5f;
        
        [SerializeField]
        private int _burstCount = 5;
        
        [SerializeField]
        private float _burstInterval = 0.15f;
        
        [SerializeField]
        private float _burstCooldown = 2f;

        public float ShootDamage => BaseConfig._shootDamage;
        public int BurstCount => BaseConfig._burstCount;
        public float BurstInterval => BaseConfig._burstInterval;
        public float BurstCooldown => BaseConfig._burstCooldown;
    }
}