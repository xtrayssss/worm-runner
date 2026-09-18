using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class GateView : InteractiveObjectView
    {
        [SerializeField]
        private TMP_Text _effectNameLabel;

        [SerializeField]
        private TMP_Text _effectValueLabel;

        [SerializeField]
        private SpriteRenderer _gateSprite;

        [SerializeField]
        private Sprite _buffSprite;

        [SerializeField]
        private Sprite _debuffSprite;

        public void Construct(EffectType effectType, StatId targetStat) =>
            _effectNameLabel.text = EnhancementService.GetEffectDisplayName(effectType, targetStat);

        public void UpdateEffectValueLabel(float currentValue) => 
            _effectValueLabel.text = currentValue.ToString("F0");

        public void SetSprite(bool isBuff) =>
            _gateSprite.sprite = isBuff ? _buffSprite : _debuffSprite;
    }
}