using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay.Features.AdvertisementFeature
{
    public interface IAdvertisementProvider
    {
        public bool IsRewardedAvailable();
        public bool IsRewardPlaying();
        public void ShowRewarded(Action onStart, Action<string> onRewarded, Action<bool> onClose);
        public bool IsInterstitialAvailable();
        public bool IsInterstitialPlaying();
        public void ShowInterstitial(Action onStart, Action<bool> onClose);

        public UniTask<bool> ShowInterstitialAsync(Action onStart, Action<bool> onClose,
            CancellationToken cancellationToken = default);
    }
}