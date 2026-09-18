using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay.Features.AdvertisementFeature
{
    public sealed class AdvertisementService : IService
    {
        private readonly AudioService _audioService;
        private readonly GameTimeService _gameTimeService;
        private readonly IAdvertisementProvider _provider;

#if UNITY_EDITOR
        public bool SkipAds { get; set; }
#endif

        public AdvertisementService(
            AudioService audioService,
            GameTimeService gameTimeService,
            IAdvertisementProvider provider)
        {
            _audioService = audioService;
            _gameTimeService = gameTimeService;
            _provider = provider;
        }

        public void ShowReward(
            Action<string> onRewarded,
            Action onRewardedStart = null,
            Action<bool> onRewardedClose = null,
            bool unpauseAfterClose = true)
        {
#if UNITY_EDITOR
            if (SkipAds)
            {
                onRewardedStart?.Invoke();
                onRewarded?.Invoke("reward");
                onRewardedClose?.Invoke(true);
                return;
            }
#endif

            if (!_provider.IsRewardedAvailable() || _provider.IsRewardPlaying())
                return;

            _provider.ShowRewarded(
                onStart: WrappedStart,
                onRewarded: onRewarded,
                onClose: WrappedClose);

            return;

            void WrappedStart()
            {
                _gameTimeService.Pause();
                _audioService.MuteAll();
                onRewardedStart?.Invoke();
            }

            void WrappedClose(bool success)
            {
                _audioService.UnmuteAll();

                if (unpauseAfterClose)
                    _gameTimeService.Unpause();

                onRewardedClose?.Invoke(success);
            }
        }

        public void ShowInterstitial(bool unpauseAfterClose = true)
        {
#if UNITY_EDITOR
            if (SkipAds)
                return;
#endif
            
            if (!_provider.IsInterstitialAvailable() || _provider.IsInterstitialPlaying())
                return;

            _provider.ShowInterstitial(OnInterstitialStart,
                success => OnInterstitialClosed(success, unpauseAfterClose));
        }

        public async UniTask<bool> ShowInterstitialAsync(bool unpauseAfterClose = true,
            CancellationToken cancellationToken = default)
        {
#if UNITY_EDITOR
            if (SkipAds)
                return true;
#endif
            
            if (!_provider.IsInterstitialAvailable() || _provider.IsInterstitialPlaying())
                return false;

            return await _provider.ShowInterstitialAsync(
                OnInterstitialStart,
                success => OnInterstitialClosed(success, unpauseAfterClose),
                cancellationToken);
        }

        private void OnInterstitialStart()
        {
            _gameTimeService.Pause();
            _audioService.MuteAll();
        }

        private void OnInterstitialClosed(bool success, bool unpauseAfterClose)
        {
            _audioService.UnmuteAll();

            if (unpauseAfterClose)
                _gameTimeService.Unpause();
        }
    }
}
