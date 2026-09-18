using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.GameFeature.Configs
{
    [CreateAssetMenu(fileName = nameof(GameConfig),
        menuName = ProjectConfig.PROJECT_NAME + "/Configs/" + nameof(GameConfig))]
    public sealed class GameConfig : SerializedScriptableObject
    {
        [SerializeField]
        public Dictionary<CurrencyType, CurrencyAmountIcons> CurrencyAmountIcons;

        [NonSerialized]
        [OdinSerialize]
        private EffectIconsConfig _effectIcons;

        [SerializeField]
        private VFXData _upgradeVFX;

        [field: SerializeField]
        public Sprite QuestionMarkIcon { get; private set; }

        public VFXData UpgradeVFX => _upgradeVFX;

        public EffectIconsConfig EffectIcons => _effectIcons;
    }

    [Serializable]
    public sealed class CurrencyAmountIcons
    {
        [field: SerializeField] public Sprite SingleIcon { get; private set; }
        [field: SerializeField] public Sprite HandfulIcon { get; private set; }
        [field: SerializeField] public Sprite PouchIcon { get; private set; }
        [field: SerializeField] public Sprite VaultIcon { get; private set; }

        [field: SerializeField] public int HandfulThreshold { get; private set; } = 101;
        [field: SerializeField] public int PouchThreshold { get; private set; } = 2500;

        public Sprite GetIconForAmount(int amount)
        {
            if (amount >= PouchThreshold)
                return VaultIcon;
            if (amount >= HandfulThreshold)
                return PouchIcon;
            if (amount > 1)
                return HandfulIcon;

            return SingleIcon;
        }

        public Sprite GetSingleIcon()
        {
            return SingleIcon;
        }
    }

    [Serializable]
    public sealed class EffectIconsConfig
    {
        [NonSerialized]
        [OdinSerialize]
        private Dictionary<EffectType, Sprite> _effectTypeIcons = new Dictionary<EffectType, Sprite>();

        [NonSerialized]
        [OdinSerialize]
        private Dictionary<StatId, Sprite> _statIcons = new Dictionary<StatId, Sprite>();

        private Sprite GetEffectIcon(EffectType effectType) =>
            _effectTypeIcons.GetValueOrDefault(effectType);

        private Sprite GetStatIcon(StatId statId) =>
            _statIcons.GetValueOrDefault(statId);

        public Sprite GetIcon(EffectType effectType, StatId? statId = null)
        {
            if (effectType == EffectType.STAT_MODIFICATION && statId.HasValue)
                return GetStatIcon(statId.Value);

            return GetEffectIcon(effectType);
        }
    }
}