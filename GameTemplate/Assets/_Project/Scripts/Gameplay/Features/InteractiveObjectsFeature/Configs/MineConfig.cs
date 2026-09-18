using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record MineConfig : BaseInteractiveObjectConfig<MineConfig>
    {
        [ShowIf("IsStandaloneConfig")]
        [SerializeField, Min(0f)]
        private float _armingDelay = 1f;

        [ShowIf("IsStandaloneConfig")]
        [SerializeField, Min(0.1f)]
        private float _explosionRadius = 3f;

#if UNITY_EDITOR
        [ShowIf("IsStandaloneConfig")]
        [SerializeField]
        private bool _showDetectionRadius = true;
#endif

        public float ArmingDelay => BaseConfig._armingDelay;

        public float ExplosionRadius => BaseConfig._explosionRadius;

#if UNITY_EDITOR
        public bool ShowDetectionRadius => _showDetectionRadius;
#endif

#if UNITY_EDITOR
        private bool IsStandaloneConfig(Object root) =>
            root is not LevelEditor && root is not LevelConfig;
#endif
    }
}