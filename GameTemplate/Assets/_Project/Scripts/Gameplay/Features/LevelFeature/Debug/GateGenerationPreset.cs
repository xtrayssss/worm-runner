#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class GateGenerationPreset : ObjectGenerationPreset
    {
        [TabGroup("Gate Settings")]
        [SerializeField] private Vector2 _baseEffectValueRange = new Vector2(1f, 5f);

        [TabGroup("Gate Settings")]
        [SerializeField] private bool _isEnhanceable = true;

        private static readonly StatId[] AVAILABLE_STAT_IDS =
        {
            StatId.COOLDOWN,
            StatId.DAMAGE,
            StatId.SHOOTING_RANGE,
        };

        public override InteractiveObjectId ObjectType => InteractiveObjectId.GATE;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            float effectValue = Random.Range(_baseEffectValueRange.x, _baseEffectValueRange.y);

            const EffectType EFFECT_TYPE = EffectType.STAT_MODIFICATION;

            StatId targetStatId = GetRandomStatId();
            bool isDebuff = Random.value < 0.35f;

            GateConfig gateConfig = new GateConfig
            {
                EffectType = EFFECT_TYPE,
                StatConfig = new GateConfig.StatModificationConfig
                {
                    TargetStat = targetStatId
                },
                EffectValue = isDebuff ? -effectValue : effectValue,
                IsEnhanceable = _isEnhanceable
            };

            config.GateConfig = gateConfig;
        }

        private StatId GetRandomStatId()
        {
            int randomIndex = Random.Range(0, AVAILABLE_STAT_IDS.Length);
            return AVAILABLE_STAT_IDS[randomIndex];
        }

        [Serializable]
        public class StatModificationPreset
        {
            [SerializeField] public StatId TargetStat;
        }
    }
}
#endif