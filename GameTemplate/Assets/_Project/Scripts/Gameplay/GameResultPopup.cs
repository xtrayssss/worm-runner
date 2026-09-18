using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameResultPopup : ModularWindow
    {
        [SerializeField]
        private RectTransform _moneyStatLabel;

        [SerializeField]
        private RectTransform _distanceStatLabel;

        [SerializeField]
        private RectTransform _crowdMembersLostLabel;

        [SerializeField]
        private TextMeshProUGUI _moneyCollectedText;

        [SerializeField]
        private TextMeshProUGUI _distanceTraveledText;

        [SerializeField]
        private TextMeshProUGUI _crowdMembersLostText;

        [SerializeField]
        private Image _topPlateImage;

        [SerializeField]
        private Sprite _victoryPlateSprite;

        [SerializeField]
        private Sprite _defeatPlateSprite;

        [SerializeField]
        private Image _resultTextImage;

        [SerializeField]
        private Sprite _victoryTextSprite;

        [SerializeField]
        private Sprite _defeatTextSprite;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private RewardedAdButton _doubleRewardButton;

        [SerializeField]
        private ShineEffect _doubleRewardButtonShine;

        private Action _onClosed;
        private Action _onDoubleRewardGranted;

        private bool _hasClaimedDoubleReward;
        private UIElementTweener.UIElement[] _statLabels;
        private Vector3 _closeButtonOriginalScale;
        private Vector3 _doubleRewardButtonOriginalScale;

        public void Construct(
            bool isVictory,
            int collectedMoney,
            int crowdMembersLost,
            float distanceTraveled,
            Action onDoubleRewardGranted,
            AdvertisementService advertisementService,
            Action onClosed = null)
        {
            _onClosed = onClosed;
            _onDoubleRewardGranted = onDoubleRewardGranted;
            _doubleRewardButton.Construct(advertisementService);
            _closeButtonOriginalScale = _closeButton.transform.localScale;
            _doubleRewardButtonOriginalScale = _doubleRewardButton.transform.localScale;

            _statLabels = new[]
            {
                new UIElementTweener.UIElement
                {
                    RectTransform = _moneyStatLabel,
                    CanvasGroup = _moneyStatLabel.GetComponent<CanvasGroup>()
                },
                new UIElementTweener.UIElement
                {
                    RectTransform = _distanceStatLabel,
                    CanvasGroup = _distanceStatLabel.GetComponent<CanvasGroup>()
                },
                new UIElementTweener.UIElement
                {
                    RectTransform = _crowdMembersLostLabel,
                    CanvasGroup = _crowdMembersLostLabel.GetComponent<CanvasGroup>()
                }
            };


            SetResultVisuals(isVictory);
            UpdateCollectedMoney(collectedMoney);
            UpdateDistanceTraveled(distanceTraveled);
            UpdateCrowdMembersLost(crowdMembersLost);
            SetupButtons();
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public override UniTask ShowAsync()
        {
            base.ShowAsync();

            foreach (UIElementTweener.UIElement label in _statLabels)
                label.CanvasGroup.alpha = 0f;

            _closeButton.transform.localScale = Vector3.zero;
            _doubleRewardButton.transform.localScale = Vector3.zero;

            WindowTweener
                .ShowGrowBounceWindow(
                    ContentRectTransform,
                    canvasGroup: SelfCanvasGroup,
                    useUnscaledTime: true)
                .Chain(AnimateStatLabels())
                .Chain(Tween.Scale(
                    _closeButton.transform,
                    _closeButtonOriginalScale,
                    duration: 0.3f,
                    Ease.OutBack))
                .Group(Tween.Scale(
                    _doubleRewardButton.transform,
                    _doubleRewardButtonOriginalScale,
                    duration: 0.3f,
                    Ease.OutBack))
                .OnComplete(this, static window => window.EnableInteraction());

            return UniTask.CompletedTask;
        }

        public override async UniTask CloseAsync(CancellationToken externCancellationToken = default)
        {
            CancellationToken linkedToken = CreateLinkedCancellationToken(externCancellationToken);

            base.CloseAsync(linkedToken);

            await WindowTweener
                .HideGrowBounceWindow(
                    ContentRectTransform,
                    canvasGroup: SelfCanvasGroup,
                    useUnscaledTime: true)
                .OnComplete(this, static window => window.EnableInteraction())
                .AsTask(linkedToken);
        }

        private Sequence AnimateStatLabels() =>
            UIElementTweener.StaggeredPop(
                _statLabels,
                animationDuration: 0.17f);

        private void UpdateDistanceTraveled(float distance) =>
            _distanceTraveledText.text = $"{Mathf.RoundToInt(distance)}m";

        public void UpdateCollectedMoney(int collectedMoney) =>
            _moneyCollectedText.text = collectedMoney.ToString();

        private void SetResultVisuals(bool isVictory)
        {
            _topPlateImage.sprite = isVictory ? _victoryPlateSprite : _defeatPlateSprite;
            _resultTextImage.sprite = isVictory ? _victoryTextSprite : _defeatTextSprite;
        }

        private void UpdateCrowdMembersLost(int crowdMembersLost) =>
            _crowdMembersLostText.text = crowdMembersLost.ToString();

        private void SetupButtons()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);

            _doubleRewardButton.OnRewardGranted += OnDoubleRewardGranted;
            _doubleRewardButton.ResetState();
        }

        private void OnCloseButtonClicked() =>
            _onClosed?.Invoke();

        private void OnDoubleRewardGranted()
        {
            if (_hasClaimedDoubleReward)
                return;

            _hasClaimedDoubleReward = true;

            _doubleRewardButtonShine.StopShine();

            _onDoubleRewardGranted?.Invoke();
        }
    }
}