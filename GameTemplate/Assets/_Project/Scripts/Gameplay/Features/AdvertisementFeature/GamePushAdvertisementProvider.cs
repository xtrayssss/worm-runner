#if GAMEPUSH_ENABLED
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePush;

namespace _Project.Scripts.Gameplay.Features.AdvertisementFeature
{
    public sealed class GamePushAdvertisementProvider : IAdvertisementProvider
    {
        public bool IsRewardedAvailable() =>
            GP_Ads.IsRewardedAvailable();

        public bool IsRewardPlaying() =>
            GP_Ads.IsRewardPlaying();

        public void ShowRewarded(Action onStart, Action<string> onRewarded, Action<bool> onClose)
        {
#if UNITY_EDITOR
            onStart?.Invoke();

            TimeSpan delay = TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f));

            UniTask
                .Delay(delay, ignoreTimeScale: true)
                .ContinueWith(() =>
                {
                    GP_Ads.ShowRewarded(onRewardedReward: onRewarded);
                    onClose?.Invoke(true);
                })
                .Forget();
#else
            GP_Ads.ShowRewarded(
                onRewardedStart: onStart,
                onRewardedReward: onRewarded,
                onRewardedClose: onClose);
#endif
        }

        public bool IsInterstitialAvailable() =>
            GP_Ads.IsFullscreenAvailable();

        public bool IsInterstitialPlaying() =>
            GP_Ads.IsFullscreenPlaying();

        public void ShowInterstitial(Action onStart, Action<bool> onClose) =>
            GP_Ads.ShowFullscreen(onStart, onClose);

        public async UniTask<bool> ShowInterstitialAsync(Action onStart, Action<bool> onClose,
            CancellationToken cancellationToken = default)
        {
            UniTaskCompletionSource<bool> taskCompletionSource = new UniTaskCompletionSource<bool>();

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());

#if UNITY_EDITOR
            onStart?.Invoke();
            await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f)),
                cancellationToken: cancellationToken, ignoreTimeScale: true);
            onClose?.Invoke(true);
            return true;
#else
            GP_Ads.ShowFullscreen(
                onFullscreenStart: onStart,
                onFullscreenClose: success =>
                {
                    onClose?.Invoke(success);
                    taskCompletionSource.TrySetResult(success);
                });

            return await taskCompletionSource.Task;
#endif
        }
    }
}
#endif