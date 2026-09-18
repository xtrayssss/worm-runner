using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TutorialFeature.CutoutMask.Scripts
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/Tutorial Highlight")]
    public class TutorialHighlight : MonoBehaviour
    {
        [SerializeField] 
        private TutorialFadeImage _tutorialFade;
       
        private TutorialHole _hole;

        private Tween _currentTween;

        [SerializeField]
        private float _fadeDuration = 0.3f;

        [SerializeField]
        private Ease _fadeEase = Ease.OutCubic;

        private GameObject _overlay;

        private TutorialHole Hole
        {
            get
            {
                if (_hole == null)
                    InitializeHole();

                return _hole;
            }
        }

        public void Construct(TutorialFadeImage fadeImage,
            GameObject overlay,
            float edgeSoftness,
            float maskFadeDuration = 0.3f,
            Ease maskFadeEase = Ease.OutCubic)
        {
            _overlay = overlay;
            _tutorialFade = fadeImage;
            _fadeDuration = maskFadeDuration;
            _fadeEase = maskFadeEase;

            if (_tutorialFade != null && !_tutorialFade.gameObject.activeSelf)
                _tutorialFade.gameObject.SetActive(true);

            if (isActiveAndEnabled && _tutorialFade != null)
            {
                _tutorialFade.AddHole(Hole);
                AnimateMaskFade(1f, edgeSoftness);
                overlay.SetActive(true);
            }
        }

        private void AnimateMaskFade(float fromValue, float toValue)
        {
            _currentTween.Stop();

            _currentTween = Tween.Custom(
                startValue: fromValue,
                endValue: toValue,
                duration: _fadeDuration,
                ease: _fadeEase,
                target: _tutorialFade,
                onValueChange: static (fade, value) => fade.Smoothness = value,
                useUnscaledTime: true
            ).OnComplete(this, static highlight => highlight._overlay.SetActive(false), warnIfTargetDestroyed: false);
        }

        private void InitializeHole()
        {
            if (TryGetComponent(out RectTransform rectTransform))
                _hole = new RectTransformTutorialHole(rectTransform);
            else if (TryGetComponent(out Renderer rendererComponent) && _tutorialFade != null)
                _hole = new RendererTutorialHole(rendererComponent, _tutorialFade);
        }

        public void OnEnable()
        {
            if (_tutorialFade != null)
                _tutorialFade.AddHole(Hole);
        }

        public void OnDisable()
        {
            if (_tutorialFade != null)
                _tutorialFade.RemoveHole(Hole);
        }

        public void OnDestroy()
        {
            if (_tutorialFade != null)
                _tutorialFade.RemoveHole(Hole);
        }
    }
}