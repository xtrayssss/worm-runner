using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Configs;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature.Behaviours
{
    public sealed class UpgradeCardsPanel : MonoBehaviour
    {
        [SerializeField] 
        private Transform _cardsContainer;
       
        [SerializeField] 
        private UpgradeCard _cardPrefab;
        
        private readonly List<UpgradeCard> _cards = new List<UpgradeCard>();

        public void Construct(
            ConfigsService configsService,
            CurrencyService currencyService,
            EnhancementService enhancementService)
        {
            Dictionary<UpgradeType, UpgradeConfig>.ValueCollection upgrades = configsService.GetAllUpgradeConfigs();

            foreach (UpgradeConfig upgrade in upgrades)
            {
                UpgradeCard card = Instantiate(_cardPrefab, _cardsContainer);

                CrowdConfig crowdConfig = configsService.GetCrowdConfig();

                card.Construct(
                    upgrade,
                    crowdConfig.PipsPerTier,
                    currencyService,
                    enhancementService,
                    maxCrowdEvolutionLevel: crowdConfig.MaxEvolutionLevel);
                
                _cards.Add(card);
            }
        }
        
        public void Refresh()
        {
            foreach (UpgradeCard card in _cards) 
                card.UpdateCardState();
        }
    }
}