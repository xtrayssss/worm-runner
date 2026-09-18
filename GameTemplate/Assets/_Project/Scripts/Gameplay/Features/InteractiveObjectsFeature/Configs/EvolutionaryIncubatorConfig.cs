using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record EvolutionaryIncubatorConfig : BaseInteractiveObjectConfig<EvolutionaryIncubatorConfig>
    {
        [TabGroup("Basic")]
        [SerializeField, Min(1)]
        private int _spawnCount = 1;

        [TabGroup("Enhancement")]
        [SerializeField]
        private bool _isEnhanceable = true;

        [TabGroup("Enhancement")]
        [ShowIf("@IsStandaloneConfig($root) && _isEnhanceable")]
        [SerializeField]
        private float[] _damageThresholdsPerLevel = { 50f, 100f, 150f, 200f, 250f };

        public int SpawnCount
        {
            get => _spawnCount;
            set => _spawnCount = value;
        }

        public bool IsEnhanceable
        {
            get => _isEnhanceable;
            set => _isEnhanceable = value;
        }

        public float[] DamageThresholdsPerLevel => BaseConfig._damageThresholdsPerLevel;

#if UNITY_EDITOR
        private bool IsStandaloneConfig(Object root) =>
            root is not LevelEditor;
#endif
    }
}