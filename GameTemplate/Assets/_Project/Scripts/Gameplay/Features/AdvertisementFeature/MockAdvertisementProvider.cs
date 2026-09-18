using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.AdvertisementFeature
{
    public sealed class MockAdvertisementProvider : IAdvertisementProvider
    {
        private bool _isRewardedPlaying;
        private bool _isInterstitialPlaying;

        private readonly MockAdPopup _mockAdPopupPrefab =
            Resources.Load<MockAdPopup>("Prefabs/MockAdPopup"); 

        private readonly float _minLoadDelay;
        private readonly float _maxLoadDelay;
        private readonly float _loadFailureChance;

        public MockAdvertisementProvider(
            float minLoadDelay = 0.5f,
            float maxLoadDelay = 2f,
            float loadFailureChance = 0.1f)
        {
            _minLoadDelay = minLoadDelay;
            _maxLoadDelay = maxLoadDelay;
            _loadFailureChance = Mathf.Clamp01(loadFailureChance);
        }

        public bool IsRewardedAvailable() =>
            true;

        public bool IsRewardPlaying() =>
            _isRewardedPlaying;

        public bool IsInterstitialAvailable() =>
            true;

        public bool IsInterstitialPlaying() =>
            _isInterstitialPlaying;

        public void ShowRewarded(Action onStart, Action<string> onRewarded, Action<bool> onClose)
        {
            ShowRewardedAsync(onStart, onRewarded, onClose, CancellationToken.None).Forget();
        }

        public void ShowInterstitial(Action onStart, Action<bool> onClose)
        {
            ShowInterstitialInternalAsync(onStart, onClose, CancellationToken.None).Forget();
        }

        public async UniTask<bool> ShowInterstitialAsync(Action onStart, Action<bool> onClose,
            CancellationToken cancellationToken = default)
        {
            return await ShowInterstitialInternalAsync(onStart, onClose, cancellationToken);
        }

        private async UniTaskVoid ShowRewardedAsync(Action onStart, Action<string> onRewarded, Action<bool> onClose,
            CancellationToken cancellationToken)
        {
            _isRewardedPlaying = true;

            float loadDelay = UnityEngine.Random.Range(_minLoadDelay, _maxLoadDelay);
            bool loadSuccess = UnityEngine.Random.value > 0f;

            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(loadDelay), 
                    cancellationToken: cancellationToken,
                    ignoreTimeScale: true);
            }
            catch (OperationCanceledException)
            {
                L.Log("[MockAd] Rewarded ad loading cancelled");
                _isRewardedPlaying = false;
                onClose?.Invoke(false);
                return;
            }

            if (!loadSuccess)
            {
                L.LogWarning("[MockAd] Failed to load rewarded ad (simulated network error)");
                _isRewardedPlaying = false;
                onClose?.Invoke(false);
                return;
            }

            L.Log("[MockAd] Rewarded ad loaded successfully, showing now");

            MockAdPopup mockAdPopup = CreateMockAdPopup();

            mockAdPopup.ShowRewardedAd(
                onStart: onStart,
                onRewarded: () => onRewarded?.Invoke("mock_reward"),
                onClose: success =>
                {
                    _isRewardedPlaying = false;
                    onClose?.Invoke(success);
                    Object.Destroy(mockAdPopup.gameObject);
                }
            );
        }

        private async UniTask<bool> ShowInterstitialInternalAsync(Action onStart, Action<bool> onClose,
            CancellationToken cancellationToken)
        {
            _isInterstitialPlaying = true;

            float loadDelay = UnityEngine.Random.Range(_minLoadDelay, _maxLoadDelay);
            bool loadSuccess = UnityEngine.Random.value > _loadFailureChance;

            L.Log($"[MockAd] Loading interstitial ad... (delay: {loadDelay:F2}s, will succeed: {loadSuccess})");

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(loadDelay), cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                L.Log("[MockAd] Interstitial ad loading cancelled");
                _isInterstitialPlaying = false;
                onClose?.Invoke(false);
                return false;
            }

            if (!loadSuccess)
            {
                L.LogWarning("[MockAd] Failed to load interstitial ad (simulated network error)");
                _isInterstitialPlaying = false;
                onClose?.Invoke(false);
                return false;
            }

            L.Log("[MockAd] Interstitial ad loaded successfully, showing now");

            MockAdPopup mockAdPopup = CreateMockAdPopup();

            bool result = await mockAdPopup
                .ShowInterstitialAdAsync(
                    onStart: onStart,
                    onClose: success =>
                    {
                        _isInterstitialPlaying = false;
                        onClose?.Invoke(success);
                        Object.Destroy(mockAdPopup.gameObject);
                    },
                    cancellationToken
                );

            return result;
        }

        private MockAdPopup CreateMockAdPopup()
        {
            MockAdPopup mockAdPopup = Object.Instantiate(_mockAdPopupPrefab);
            mockAdPopup.Construct();
            Object.DontDestroyOnLoad(mockAdPopup);
            return mockAdPopup;
        }
    }
}