using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.AdvertisementFeature
{
    public sealed class MockAdPopup : MonoBehaviour
    {
        [SerializeField]
        private Canvas _canvas;

        [SerializeField]
        private GameObject _adPanel;

        [SerializeField]
        private Text _titleText;

        [SerializeField]
        private Text _timerText;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Image _adImage;

        [SerializeField]
        private Image _progressBar;

        private bool _rewardClaimed;

        public void Construct()
        {
            _canvas.enabled = false;
        }

        public void ShowRewardedAd(Action onStart, Action onRewarded, Action<bool> onClose)
        {
            StartCoroutine(ShowRewardedAdCoroutine(onStart, onRewarded, onClose));
        }

        public void ShowInterstitialAd(Action onStart, Action<bool> onClose)
        {
            StartCoroutine(ShowInterstitialAdCoroutine(onStart, onClose));
        }

        public async UniTask<bool> ShowInterstitialAdAsync(Action onStart, Action<bool> onClose,
            CancellationToken cancellationToken)
        {
            UniTaskCompletionSource<bool> tcs = new UniTaskCompletionSource<bool>();

            StartCoroutine(ShowInterstitialAdCoroutine(onStart, success =>
            {
                onClose?.Invoke(success);
                tcs.TrySetResult(success);
            }));

            return await tcs.Task;
        }

        private IEnumerator ShowRewardedAdCoroutine(Action onStart, Action onRewarded, Action<bool> onClose)
        {
            onStart?.Invoke();

            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() =>
            {
                _canvas.enabled = false;
                onClose?.Invoke(_rewardClaimed);

                if (_rewardClaimed)
                    onRewarded?.Invoke();
            });

            _canvas.enabled = true;
            _titleText.text = "REWARDED ADVERTISEMENT";
            _adImage.color = new Color(0.8f, 1f, 0.8f);

            const float REWARD_DURATION = 1.5f;
            float timer = REWARD_DURATION;
            while (timer > 0)
            {
                _timerText.text = $"Reward available in: {Mathf.Ceil(timer)}s";
                _progressBar.fillAmount = timer / REWARD_DURATION;
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }

            _timerText.text = "Reward Available!";

            _rewardClaimed = true;
        }

        private IEnumerator ShowInterstitialAdCoroutine(Action onStart, Action<bool> onClose)
        {
            onStart?.Invoke();

            _canvas.enabled = true;
            _titleText.text = "INTERSTITIAL ADVERTISEMENT";
            _adImage.color = new Color(1f, 0.8f, 0.8f);

            bool adClosed = false;

            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() =>
            {
                adClosed = true;
                _canvas.enabled = false;
                onClose?.Invoke(true);
            });

            const float DURATION = 1.5f;
            float timer = DURATION;

            while (timer > 0)
            {
                _timerText.text = $"Skip available in: {Mathf.Ceil(timer)}s";
                _progressBar.fillAmount = timer / DURATION;
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }

            _timerText.text = "You can skip now";

            while (!adClosed)
            {
                yield return null;
            }
        }
    }
}