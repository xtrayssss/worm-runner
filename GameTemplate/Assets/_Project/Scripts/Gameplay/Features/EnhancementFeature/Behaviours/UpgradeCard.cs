using System;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Configs;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.TutorialFeature;
using JetBrains.Annotations;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Behaviours
{
    public sealed class UpgradeCard : MonoBehaviour
    {
        [SerializeField]
        private Button _selectButton;

        [SerializeField]
        private Image _iconImage;

        [SerializeField]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        private TextMeshProUGUI _levelText;

        [SerializeField]
        private Image _backgroundImage;

        [SerializeField]
        private Image _disabledOverlay;

        [SerializeField]
        private GameObject _maxLevelOverlay;

        [SerializeField]
        private TextMeshProUGUI _costText;

        [Header("Evolution Pips")]
        [SerializeField]
        private GameObject _pipsContainer;

        [SerializeField]
        private PipView[] _pips;

        private CurrencyService _currencyService;

        private UpgradeConfig _config;
        private EnhancementService _enhancementService;
        private IDisposable _upgradeAppliedSubscribe;
        private int _pipsPerTier;
        private int _maxCrowdEvolutionLevel;

        public void Construct(UpgradeConfig config,
            int pipsPerTier,
            CurrencyService currencyService,
            EnhancementService enhancementService,
            int maxCrowdEvolutionLevel)
        {
            _maxCrowdEvolutionLevel = maxCrowdEvolutionLevel;
            _config = config;
            _pipsPerTier = pipsPerTier;
            _currencyService = currencyService;
            _enhancementService = enhancementService;

            _iconImage.sprite = config.Icon;
            _titleText.text = config.Title;
            _backgroundImage.color = config.CardColor;

            _currencyService.OnCurrencyChanged += OnCurrencyChanged;
            _upgradeAppliedSubscribe = World.Default.GetEvent<UpgradeAppliedEvent>().Subscribe(UpdatePips);

            _selectButton.onClick.AddListener(OnCardSelected);

            SetupTutorialElement();

            UpdateCardState();

            SetupPips();
            UpdatePips();
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveListener(OnCardSelected);
            _currencyService.OnCurrencyChanged -= OnCurrencyChanged;
            _upgradeAppliedSubscribe.Dispose();
        }

        private void SetupPips()
        {
            bool isEvolutionCard = _config.Type == UpgradeType.EVOLUTION && !IsMaxLevel();

            _pipsContainer.SetActive(isEvolutionCard);
        }

        private void UpdatePips([CanBeNull] FastList<UpgradeAppliedEvent> _ = null)
        {
            if (IsMaxLevel())
                return;

            for (int i = 0; i < _pips.Length; i++)
            {
                EnhancementService.UpgradeData evolutionUpgradeData =
                    _enhancementService.Upgrades[UpgradeType.EVOLUTION];

                int evolutionLevel = evolutionUpgradeData.Level;

                _pips[i].SetSprite(isActive: i < GetActivePips(evolutionLevel));
            }
        }

        private int GetActivePips(int evolutionLevel)
        {
            if (_pipsPerTier <= 0)
                return 0;

            int divisor = _pipsPerTier;

            int adjusted = (evolutionLevel - 1) % divisor;
            return adjusted + 1;
        }

        private void OnCardSelected()
        {
            EnhancementService.UpgradeData upgradeData = _enhancementService.Upgrades[_config.Type];

            int cost = _config.GetCost(upgradeData.Level);

            if (!_currencyService.HasEnough(CurrencyType.MONEY, cost))
                return;

            World.Default
                .GetRequest<ApplyUpgradeRequest>()
                .Publish(new ApplyUpgradeRequest
                    {
                        UpgradeType = _config.Type,
                        Value = _config.GetValue(),
                        IntValue = _config.GetIntValue(),
                        Cost = cost,
                        UpgradeSource = default
                    },
                    allowNextFrame: true);
        }

        private void OnCurrencyChanged(CurrencyService.CurrencyUpdateInfo currencyInfo)
        {
            if (currencyInfo.CurrencyType == CurrencyType.MONEY)
                UpdateCardState();
        }

        public void UpdateCardState()
        {
            EnhancementService.UpgradeData upgradeData = _enhancementService.Upgrades[_config.Type];

            bool isMaxLevel = false;

            if (_config.Type == UpgradeType.EVOLUTION)
            {
                isMaxLevel = IsMaxLevel();

                if (isMaxLevel)
                    _pipsContainer.SetActive(false);
            }

            int cost = _config.GetCost(upgradeData.Level);

            _costText.text = isMaxLevel ? string.Empty : $"{cost}";
            
            bool hasEnoughMoney = _currencyService.HasEnough(CurrencyType.MONEY, cost);
            _selectButton.interactable = hasEnoughMoney && !isMaxLevel;
            _disabledOverlay.gameObject.SetActive(!hasEnoughMoney && !isMaxLevel);
            _maxLevelOverlay.SetActive(isMaxLevel);

            _levelText.text = $"Level {upgradeData.Level}";
        }

        private void SetupTutorialElement()
        {
            TutorialDynamicElement element = gameObject.AddComponent<TutorialDynamicElement>();

            element.ElementKey = _config.Type switch
            {
                UpgradeType.POPULATION => TutorialElementKeys.UPGRADE_CARD_POPULATION,
                UpgradeType.ATTACK => TutorialElementKeys.UPGRADE_CARD_ATTACK,
                UpgradeType.EVOLUTION => TutorialElementKeys.UPGRADE_CARD_EVOLUTION,
                UpgradeType.INCOME => TutorialElementKeys.UPGRADE_CARD_INCOME,
                _ => throw new ArgumentOutOfRangeException(nameof(_config.Type), _config.Type, null)
            };
        }

        private bool IsMaxLevel()
        {
            EnhancementService.UpgradeData currentUpgradeData = _enhancementService.Upgrades[UpgradeType.EVOLUTION];
            return currentUpgradeData.Level >= _maxCrowdEvolutionLevel;
        }
    }
}