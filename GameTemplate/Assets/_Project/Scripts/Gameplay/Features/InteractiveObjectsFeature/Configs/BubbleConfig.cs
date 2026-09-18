using System;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record BubbleConfig : BaseInteractiveObjectConfig<BubbleConfig>
    {
        [SerializeField]
        private EffectType _effectType;

        [SerializeField]
        private float _effectValue;

        [ShowIf("EffectType", optionalValue: EffectType.STAT_MODIFICATION)]
        [SerializeField] private StatModificationConfig _statConfig;

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
    }
}