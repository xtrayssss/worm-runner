using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LoadingScreenFeature
{
    public class LoadingCurtain : MonoBehaviour, IService
    {
        [SerializeField]
        private float _fadeDuration = 0.5f;

        [SerializeField]
        private Ease _fadeInEase = Ease.OutCubic;

        [SerializeField]
        private Ease _fadeOutEase = Ease.InCubic;

        [SerializeField]
        private RectTransform _content;

        [SerializeField]
        private LoadingScreenAnimation _loadingAnimation;

        private CanvasGroup _canvasGroup;
        private Tween _fadeTween;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        [Button]
        public void Show(bool withFadeAnimation = false) =>
            ShowAsync(withFadeAnimation).Forget();

        [Button]
        public void Hide() =>
            HideAsync().Forget();

        public async UniTask ShowAsync(bool withFadeAnimation = false)
        {
            _fadeTween.Stop();

            _canvasGroup.blocksRaycasts = true;

            gameObject.SetActive(true);
            EnableContent();

            if (!_loadingAnimation.IsInitialized)
            {
                _loadingAnimation.Construct();
                _loadingAnimation.gameObject.SetActive(true);
            }

            _loadingAnimation.PlayAnimation();

            if (withFadeAnimation)
            {
                _fadeTween = Tween.Alpha(
                    _canvasGroup,
                    0,
                    1,
                    _fadeDuration,
                    _fadeInEase);

                await _fadeTween.ToYieldInstruction().ToUniTask();
            }
            else
            {
                _canvasGroup.alpha = 1;
            }
        }

        public async UniTask HideAsync() => 
            await FadeOut();

        private async UniTask FadeOut()
        {
            _fadeTween.Stop();

            _fadeTween = Tween.Alpha(
                    _canvasGroup,
                    _canvasGroup.alpha,
                    0,
                    _fadeDuration,
                    _fadeOutEase)
                .OnComplete(this, static curtain =>
                {
                    curtain.gameObject.SetActive(false);
                    curtain._canvasGroup.blocksRaycasts = false;
                    curtain._loadingAnimation.StopAnimation();
                });

            await _fadeTween.ToYieldInstruction();
        }

        private void EnableContent() =>
            _content.gameObject.SetActive(true);
    }
}