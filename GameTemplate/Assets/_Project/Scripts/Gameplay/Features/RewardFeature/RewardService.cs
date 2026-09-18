using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.GameFeature.Configs;
using _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.RewardFeature
{
    [Serializable]
    public sealed class RewardService : IService
    {
        private readonly ConfigsService _configsService;
        private readonly CurrencyService _currencyService;
        private readonly GameConfig _gameConfig;

        public static readonly RewardCardOrder[] FRAGMENT_FIRST_ORDER =
        {
            RewardCardOrder.FRAGMENTS,
            RewardCardOrder.GOLD,
            RewardCardOrder.CRYSTALS,
            RewardCardOrder.MONEY
        };

        public enum RewardCardOrder
        {
            GOLD = 0,
            CRYSTALS = 1,
            FRAGMENTS = 2,
            MONEY = 3
        }

        public RewardService(ConfigsService configsService, CurrencyService currencyService)
        {
            _configsService = configsService;
            _currencyService = currencyService;
            _gameConfig = configsService.GetGameConfig();
        }

        public void GrantRewards(Rewards rewards, float multiplier = 1f, bool queueAnimation = false)
        {
            Rewards multipliedRewards = rewards.Multiply(multiplier);

            GrantRewardsInternal(multipliedRewards, queueAnimation);
        }

        private void GrantRewardsInternal(Rewards rewards, bool queueAnimation = false)
        {
            if (rewards?.AllRewards == null)
                return;

            foreach (IReward reward in rewards.AllRewards)
            {
                if (reward is CurrencyReward currencyReward)
                {
                    if (currencyReward.Amount.FinalValue <= 0)
                        continue;

                    switch (currencyReward.CurrencyType)
                    {
                        case CurrencyType.MONEY:
                            _currencyService.AddMoney(currencyReward.Amount.FinalValue, queueAnimation);
                            break;
                        case CurrencyType.GEMS:
                            _currencyService.AddGems(currencyReward.Amount.FinalValue, queueAnimation);
                            break;
                    }
                }
                else if (reward is FragmentsReward fragmentsReward)
                {
                    foreach (KeyValuePair<int, RewardValue> kvp in fragmentsReward.Fragments)
                    {
                        if (kvp.Value.FinalValue > 0)
                        {
                            //_currencyService.AddFragments(kvp.Key, kvp.Value.FinalValue, queueAnimation);
                        }
                    }
                }
            }

#if DEBUG
            LogRewardGrant(rewards);
#endif
        }

        public List<RewardCard> CreateRewardCards(
            Rewards rewards,
            Transform container,
            bool isDoubleRewards = false,
            List<RewardCard> cardsBuffer = null,
            bool makeInteractive = false,
            float scale = 1.0f,
            bool useSingleCard = false,
            bool hideType = false,
            RewardCardOrder[] customOrder = null)
        {
            List<RewardCard> cards = new List<RewardCard>();
            if (rewards?.AllRewards == null) return cards;

            IEnumerable<IReward> orderedRewards = customOrder != null
                ? OrderRewardsByCustomOrder(rewards, customOrder)
                : rewards.AllRewards.OrderBy(GetRewardOrderPriority);

            foreach (IReward reward in orderedRewards)
            {
                if (reward is FragmentsReward fragmentsReward)
                {
                    cards.AddRange(CreateFragmentsRewardCards(fragmentsReward, container, isDoubleRewards, cardsBuffer,
                        makeInteractive, scale, useSingleCard, hideType));
                }
                else if (reward is CurrencyReward currencyReward)
                {
                    RewardCard card = CreateCurrencyRewardCard(currencyReward, container, isDoubleRewards,
                        makeInteractive, scale);
                    if (card != null)
                    {
                        cards.Add(card);
                        cardsBuffer?.Add(card);
                    }
                }
            }

            return cards;
        }

        private IEnumerable<IReward> OrderRewardsByCustomOrder(Rewards rewards, RewardCardOrder[] customOrder)
        {
            List<IReward> orderedRewards = new List<IReward>();

            foreach (RewardCardOrder orderType in customOrder)
            {
                IEnumerable<IReward> matchingRewards =
                    rewards.AllRewards.Where(r => GetRewardOrderType(r) == orderType);

                orderedRewards.AddRange(matchingRewards);
            }

            return orderedRewards;
        }

        private static RewardCardOrder GetRewardOrderType(IReward reward)
        {
            return reward.Type switch
            {
                RewardType.GOLD => RewardCardOrder.GOLD,
                RewardType.CRYSTAL => RewardCardOrder.CRYSTALS,
                RewardType.FRAGMENT => RewardCardOrder.FRAGMENTS,
                RewardType.MONEY => RewardCardOrder.MONEY,
                _ => RewardCardOrder.GOLD
            };
        }

        private static int GetRewardOrderPriority(IReward reward)
        {
            return reward.Type switch
            {
                RewardType.CRYSTAL => 0,
                RewardType.GOLD => 1,
                RewardType.FRAGMENT => 2,
                RewardType.MONEY => 3,
                _ => 3
            };
        }

        private RewardCard CreateCurrencyRewardCard(
            CurrencyReward reward,
            Transform container,
            bool isDoubleRewards,
            bool makeInteractive,
            float scale)
        {
            if (reward.Amount.IsZero()) return null;

            RewardCard prefab = _configsService.GetCurrencyRewardCard();
            RewardCard card = Object.Instantiate(prefab, container);
            card.transform.localScale = Vector3.one * scale;
            card.Construct();

            RewardCard.RewardDetail detail = new RewardCard.RewardDetail
            {
                Icon = _gameConfig.CurrencyAmountIcons[reward.CurrencyType].GetSingleIcon(),
                RewardType = reward.Type,
                IsUnknown = false,
                IsRange = reward.Amount.IsRange,
                FixedAmount = reward.Amount.FixedValue,
                RangeAmount = reward.Amount.RangeValue
            };

            card.DisplayReward(detail, isDoubleRewards);

            if (makeInteractive)
            {
                card.InteractionButton.gameObject.AddComponent<ButtonAnimator>()
                    .SetSettings(1.05f, 0.15f, card.RectTransform);
            }

            return card;
        }

        private List<RewardCard> CreateFragmentsRewardCards(
            FragmentsReward reward,
            Transform container,
            bool isDoubleRewards,
            List<RewardCard> cardsBuffer,
            bool makeInteractive,
            float scale,
            bool useSingleCard,
            bool hideType)
        {
            List<RewardCard> cards = new List<RewardCard>();
            if (reward?.Fragments == null || reward.IsEmpty) return cards;

            if (useSingleCard && reward.MaxTotalFragments > 0)
            {
                RewardCard card = CreateSingleFragmentCard(reward, container, isDoubleRewards, makeInteractive, scale);
                if (card != null)
                {
                    cards.Add(card);
                    cardsBuffer?.Add(card);
                }
            }
            else
            {
                foreach (KeyValuePair<int, RewardValue> kvp in reward.Fragments)
                {
                    RewardCard card = CreateFragmentCard(kvp.Key, kvp.Value, container, isDoubleRewards,
                        makeInteractive, scale, hideType);
                    if (card != null)
                    {
                        cards.Add(card);
                        cardsBuffer?.Add(card);
                    }
                }
            }

            return cards;
        }

        private RewardCard CreateSingleFragmentCard(
            FragmentsReward reward,
            Transform container,
            bool isDoubleRewards,
            bool makeInteractive,
            float scale)
        {
            RewardCard prefab = _configsService.GetFragmentRewardCard();
            RewardCard card = Object.Instantiate(prefab, container);
            card.transform.localScale = Vector3.one * scale;
            card.Construct();

            RewardCard.RewardDetail detail = new RewardCard.RewardDetail
            {
                IsRange = false,
                FixedAmount = reward.MaxTotalFragments,
                Icon = _gameConfig.QuestionMarkIcon,
                IsUnknown = true,
                RewardType = RewardType.FRAGMENT
            };

            card.DisplayReward(detail, isDoubleRewards);

            if (makeInteractive)
            {
                card.InteractionButton.gameObject.AddComponent<ButtonAnimator>()
                    .SetSettings(1.05f, 0.15f, card.RectTransform);
            }

            return card;
        }

        private RewardCard CreateFragmentCard(
            int type,
            RewardValue amount,
            Transform container,
            bool isDoubleRewards,
            bool makeInteractive,
            float scale,
            bool hideType)
        {
            if (amount.FinalValue <= 0 && !amount.IsRange) return null;

            RewardCard prefab = _configsService.GetFragmentRewardCard();
            RewardCard card = Object.Instantiate(prefab, container);
            card.transform.localScale = Vector3.one * scale;
            card.Construct();

            RewardCard.RewardDetail detail = new RewardCard.RewardDetail
            {
                IsRange = amount.IsRange,
                FixedAmount = amount.IsRange ? 0 : amount.FinalValue,
                RangeAmount = amount.IsRange ? amount.RangeValue : Vector2Int.zero,
                Icon = hideType ? _gameConfig.QuestionMarkIcon : null,
                IsUnknown = hideType,
                RewardType = RewardType.FRAGMENT
            };

            card.DisplayReward(detail, isDoubleRewards);

            if (makeInteractive)
            {
                card.InteractionButton.gameObject.AddComponent<ButtonAnimator>()
                    .SetSettings(1.05f, 0.15f, card.RectTransform);
            }

            return card;
        }

#if DEBUG
        private void LogRewardGrant(Rewards rewards)
        {
            string rewardLog = string.Join(", ", rewards.AllRewards.Select(r =>
            {
                if (r is CurrencyReward currency)
                    return $"{currency.Type}: {currency.Amount.FinalValue}";
                if (r is FragmentsReward fragments)
                    return
                        $"Fragments: {string.Join(", ", fragments.Fragments.Select(kvp => $"Type {kvp.Key}: {kvp.Value.FinalValue}"))}";
                return r.Type.ToString();
            }));
            Debug.Log($"Granted rewards: {rewardLog}");
        }
#endif
    }
}