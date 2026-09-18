using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Configs
{
    [Serializable]
    [CreateAssetMenu(fileName = nameof(ProjectileConfig),
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/" + nameof(ProjectileConfig))]
    public class ProjectileConfig : ScriptableObject
    {
        [FoldoutGroup("Projectile Settings")]
        [SerializeField, Required, AssetsOnly]
        private ProjectileId _projectileId;

        [FoldoutGroup("Projectile Settings")]
        [SerializeField, Required, AssetsOnly]
        private ProjectileView _projectilePrefab;

        [FoldoutGroup("Projectile Settings")]
        [SerializeField]
        private float _speed = 10f;

        [SerializeField]
        [FoldoutGroup("Projectile Settings")]
        private float _maxDistance = 5f;

        [FoldoutGroup("Enhancement Settings")]
        [SerializeField]
        private int _pierceCount;

        [FoldoutGroup("Enhancement Settings")]
        [SerializeField]
        private int _ricochetCount;

        [FoldoutGroup("Enhancement Settings")]
        [SerializeField, Range(0f, 1f)]
        [ProgressBar(0, 1, ColorGetter = "GetCritChanceColor")]
        private float _critChance;

        [FoldoutGroup("Enhancement Settings")]
        [SerializeField]
        [ShowIf("@_critChance > 0")]
        private float _critMultiplier = 1.5f;

        [FoldoutGroup("Enhancement Settings")]
        [SerializeField, Range(0f, 1f)]
        [ProgressBar(0, 1, ColorGetter = "GetFreezeChanceColor")]
        private float _freezeChance;

        // Properties
        public ProjectileView ProjectilePrefab => _projectilePrefab;
        public float Speed => _speed;
        public float MaxDistance => _maxDistance;
        public int PierceCount => _pierceCount;
        public int RicochetCount => _ricochetCount;
        public float CritChance => _critChance;
        public float CritMultiplier => _critMultiplier;
        public float FreezeChance => _freezeChance;
        public ProjectileId ProjectileId => _projectileId;

        private Color GetCritChanceColor()
        {
            return Color.Lerp(Color.white, Color.red, _critChance);
        }

        private Color GetFreezeChanceColor()
        {
            return Color.Lerp(Color.white, Color.cyan, _freezeChance);
        }
    }
}