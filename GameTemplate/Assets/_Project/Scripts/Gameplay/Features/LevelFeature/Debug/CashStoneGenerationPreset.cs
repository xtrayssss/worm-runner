#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class CashStoneGenerationPreset : ObjectGenerationPreset
    {
        [TabGroup("Cash Stone Settings")]
        [SerializeField] private Vector2 _maxHealthRange = new Vector2(15f, 30f);

        [TabGroup("Reward Settings")]
        [SerializeField] private Rewards _rewards;

        public override InteractiveObjectId ObjectType => InteractiveObjectId.CASH_STONE;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            config.MaxHealth = Random.Range(_maxHealthRange.x, _maxHealthRange.y);
            config.Rewards = _rewards;
        }
    }
}
#endif