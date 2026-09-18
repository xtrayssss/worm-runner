using _Project.Scripts.Gameplay.Features.TweenFeature;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    public sealed class ButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _scaleMultiplier = 1.1f;
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private RectTransform _target;

        private Vector3 _originalScale;
        private Tween _currentTween;
        private bool _isPressed = false;
        private ScrollAwareButton _scrollAwareButton;

        private void Awake()
        {
            if (_target == null)
                _target = GetComponent<RectTransform>();

            _originalScale = _target.localScale;
            _scrollAwareButton = GetComponent<ScrollAwareButton>();
        }

        public void SetSettings(float scaleMultiplier, float animationDuration, RectTransform target)
        {
            _scaleMultiplier = scaleMultiplier;
            _animationDuration = animationDuration;
            _target = target;
            
            _originalScale = _target.localScale;
        }

        private void OnEnable()
        {
            _originalScale = _target.localScale;
            _isPressed = false;
            StopAnimation();
        }

        private void OnDisable()
        {
            StopAnimation();
            _target.localScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            StopAnimation();
            AnimateButton(isScalingUp: true);

            if (_scrollAwareButton != null)
                _scrollAwareButton.OnPointerDownStarted(eventData.position);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isPressed)
            {
                _isPressed = false;
                StopAnimation();
                AnimateButton(isScalingUp: false);
            }
        }

        private void AnimateButton(bool isScalingUp)
        {
            if (isScalingUp)
            {
                _currentTween = ButtonTweener.AnimateButtonPressScale(
                    _target,
                    customDuration: _animationDuration,
                    customScale: _scaleMultiplier * _originalScale,
                    useUnscaledTime: true);
            }
            else
            {
                _currentTween = ButtonTweener.AnimateButtonReleaseScale(
                    _target,
                    customDuration: _animationDuration,
                    customScale: _originalScale,
                    useUnscaledTime: true);
            }
        }

        public void StopAnimation() =>
            _currentTween.Stop();

        public void ResetScale()
        {
            _target.localScale = _originalScale;
        }
    }
}