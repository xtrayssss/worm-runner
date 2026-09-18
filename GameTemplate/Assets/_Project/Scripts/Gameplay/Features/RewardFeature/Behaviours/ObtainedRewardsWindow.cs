using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.ConfettiFeature;
using _Project.Scripts.Gameplay.Features.ConfettiFeature.Services;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours
{
    public sealed class ObtainedRewardsWindow : ModularWindow
    {
        [SerializeField]
        private RectTransform _rewardsContainer;

        [SerializeField]
        private RectTransform _confettiSpawnPoint;

        [SerializeField]
        private Button _collectButton;

        [Header("Animations")]
        [SerializeField]
        private float _delayBetweenRewardCards = 0.1f;

        [SerializeField]
        private float _rewardCardsAnimationDuration = 0.2f;

        private List<RewardCard> _displayedRewardCards = new List<RewardCard>();
        [ShowInInspector] private Rewards _obtainedRewards;
        private Action _onRewardsCollectedCallback;
        private RewardService _rewardService;
        private ConfettiService _confettiService;
        private AudioService _audioService;

        public void Construct(
            Rewards obtainedRewards,
            RewardService rewardService,
            ConfettiService confettiService,
            Action onRewardsCollectedCallback, 
            AudioService audioService)
        {
            _obtainedRewards = obtainedRewards;
            _onRewardsCollectedCallback = onRewardsCollectedCallback;
            _rewardService = rewardService;
            _confettiService = confettiService;
            _audioService = audioService;
        }

        private void OnEnable() =>
            _collectButton.onClick.AddListener(CollectRewards);

        private void OnDisable() =>
            _collectButton.onClick.RemoveListener(CollectRewards);

        public override UniTask ShowAsync()
        {
            base.ShowAsync();

            Sequence showAnimation = WindowTweener
                .ShowZoomPunchWindow(ContentRectTransform, SelfCanvasGroup);

            showAnimation
                .InsertCallback(showAnimation.durationTotal * 0.4f, this, static window =>
                {
                    window._confettiService.PlayConfetti(
                        ConfettiId.REWARD,
                        window._confettiSpawnPoint.transform.position,
                        window.Canvas.transform);
                })
                .OnComplete(this, static window =>
                {
                    window.DisplayObtainedRewards();
                    window.EnableInteraction();
                });

            return UniTask.CompletedTask;
        }

        public override async UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);
            
            base.CloseAsync(linkedToken);

            await WindowTweener
                .HideZoomPunchWindow(ContentRectTransform, SelfCanvasGroup)
                .OnComplete(this, static window => window.EnableInteraction())
                .AsTask(cancellationToken: linkedToken);
            
            _audioService.PlaySpecialSound(AudioId.Sfx.UI.REWARD_COLLECT);
        }

        [Button]
        private void DisplayObtainedRewards()
        {
            RemoveDisplayedRewardCards();

            _displayedRewardCards = _rewardService.CreateRewardCards(
                rewards: _obtainedRewards,
                isDoubleRewards: false,
                container: _rewardsContainer);

            List<CardTweener.Card> rewardCardsForAnimation = _displayedRewardCards
                .Select(rewardCard => new CardTweener.Card
                {
                    RectTransform = rewardCard.RectTransform,
                    CanvasGroup = rewardCard.CanvasGroup
                })
                .ToList();

            CardTweener.ShowCards(
                cards: rewardCardsForAnimation,
                delayBetweenCards: _delayBetweenRewardCards,
                animationDuration: _rewardCardsAnimationDuration);
        }

        private void RemoveDisplayedRewardCards()
        {
            foreach (RewardCard rewardCard in _displayedRewardCards)
                Destroy(rewardCard.gameObject);

            _displayedRewardCards.Clear();
        }

        private void CollectRewards()
        {
            _onRewardsCollectedCallback?.Invoke();
        }
    }
}