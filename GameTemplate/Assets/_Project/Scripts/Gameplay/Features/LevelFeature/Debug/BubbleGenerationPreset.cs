#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class BubbleGenerationPreset : ObjectGenerationPreset
    {
        [TabGroup("Bubble Settings")]
        [SerializeField] private Vector2 _baseEffectValueRange = new Vector2(1f, 5f);

        private static readonly EffectType[] AVAILABLE_EFFECT_TYPES =
        {
            EffectType.STAT_MODIFICATION,
            EffectType.POPULATION_ADD
        };

        private static readonly StatId[] AVAILABLE_STAT_IDS =
        {
            StatId.COOLDOWN,
            StatId.DAMAGE
        };

        public override InteractiveObjectId ObjectType => InteractiveObjectId.BUBBLE;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            float effectValue = Random.Range(_baseEffectValueRange.x, _baseEffectValueRange.y);

            EffectType randomEffectType = GetRandomEffectType();

            BubbleConfig bubbleConfig = new BubbleConfig
            {
                EffectType = randomEffectType,
                StatConfig = new BubbleConfig.StatModificationConfig()
            };

            if (randomEffectType == EffectType.STAT_MODIFICATION)
            {
                effectValue = Random.Range(5f, 10f);

                bubbleConfig.StatConfig.TargetStat = GetRandomStatId();
            }

            bubbleConfig.EffectValue = effectValue;

            config.BubbleConfig = bubbleConfig;
        }

        private EffectType GetRandomEffectType()
        {
            int randomIndex = Random.Range(0, AVAILABLE_EFFECT_TYPES.Length);
            return AVAILABLE_EFFECT_TYPES[randomIndex];
        }

        private StatId GetRandomStatId()
        {
            int randomIndex = Random.Range(0, AVAILABLE_STAT_IDS.Length);
            return AVAILABLE_STAT_IDS[randomIndex];
        }
    }
}
#endif