using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record GateConfig : BaseInteractiveObjectConfig<GateConfig>
    {
        [TabGroup("Effect")]
        [SerializeField]
        private EffectType _effectType;

        [FormerlySerializedAs("_baseEffectValue")]
        [TabGroup("Effect")]
        [SerializeField]
        private float _effectValue;

        [TabGroup("Effect")]
        [SerializeField]
        private bool _isEnhanceable;

        [TabGroup("Effect")]
        [ShowIf("IsStandaloneConfig")]
        [SerializeField]
        private float _step = 0.02f;

        [TabGroup("Effect")]
        [ShowIf(
            "@_effectType == _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.EffectType.STAT_MODIFICATION")]
        [SerializeField]
        private StatModificationConfig _statConfig;

        public EffectType EffectType
        {
            get => _effectType;
            set => _effectType = value;
        }

        public float EffectValue
        {
            get => _effectValue;
            set => _effectValue = value;
        }

        public bool IsEnhanceable
        {
            get => _isEnhanceable;
            set => _isEnhanceable = value;
        }

        public float Step => BaseConfig._step;

        public StatModificationConfig StatConfig
        {
            get => _statConfig;
            set => _statConfig = value;
        }

        [Serializable]
        public class StatModificationConfig
        {
            [SerializeField] private StatId _targetStat;

            public StatId TargetStat
            {
                get => _targetStat;
                set => _targetStat = value;
            }
        }

#if UNITY_EDITOR
        private bool IsStandaloneConfig(Object root) =>
            root is not LevelEditor && root is not LevelConfig;
#endif
    }
}