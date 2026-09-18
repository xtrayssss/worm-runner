using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record CannonConfig : BaseInteractiveObjectConfig<CannonConfig>
    {
        [FoldoutGroup("Weapon Settings", expanded: true)]
        [ShowIf("IsStandaloneConfig")]
        [SerializeField]
        [Min(0)]
        private float _shootingCooldown = 2f;

        [FoldoutGroup("Weapon Settings")]
        [ShowIf("IsStandaloneConfig")]
        [SerializeField]
        [Min(0)]
        private float _detectionRange = 10f;

        [FoldoutGroup("Weapon Settings")]
        [ShowIf("IsStandaloneConfig")]
        [SerializeField]
        private float _projectileSpeed = 15f;

        [FoldoutGroup("Projectile Settings")]
        [ShowIf("IsStandaloneConfig")]
        [SerializeField]
        private float _projectileDamage = -10f;

        public float ShootingCooldown => BaseConfig._shootingCooldown;

        public float DetectionRange => BaseConfig._detectionRange;

        public float ProjectileDamage => BaseConfig._projectileDamage;

#if UNITY_EDITOR
        private bool IsStandaloneConfig(Object root) => 
            root is not LevelEditor && root is not LevelConfig;
#endif
    }
}