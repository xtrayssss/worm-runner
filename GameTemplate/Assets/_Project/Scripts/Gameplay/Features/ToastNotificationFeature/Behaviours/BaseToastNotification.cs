using System;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.ToastNotificationFeature.Behaviours
{
    public abstract class BaseToastNotification : BaseWindow
    {
        [SerializeField] protected TextMeshProUGUI MessageText;
        [SerializeField] protected Image BackgroundImage;

        [Header("Animation Settings")]
        [SerializeField] protected float AnimationDuration = 0.5f;

        [SerializeField] protected float DisplayDuration = 2f;
        [SerializeField, Range(0f, 0.5f)] protected float BottomOffset = 0.1f;
        [SerializeField, Range(0f, 1f)] protected float TargetYPosition = 0.5f;

        private Tween _currentAnimation;

        private CanvasGroup _canvasGroup;

        private Action _onAnimationCompleted;
        private AudioService _audioService;

        public void Initialize(WindowId id, AudioService audioService, Action onAnimationCompleted)
        {
            base.Initialize(id);

            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _audioService = audioService;

            InitializeTransform();
            gameObject.SetActive(false);

            _onAnimationCompleted = onAnimationCompleted;
        }

        protected virtual void InitializeTransform()
        {
            ContentRectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        public virtual void ShowMessage(string message)
        {
            _currentAnimation.Stop();

            MessageText.text = message;
            
            gameObject.SetActive(true);

            AnimatePopup();
            
            _audioService.PlaySound(AudioId.Sfx.UI.TOAST_NOTIFICATION);
        }

        protected virtual void AnimatePopup()
        {
            float canvasHeight = CanvasRectTransform.sizeDelta.y;
            float startY = -(canvasHeight * (0.5f - BottomOffset));
            float endY = canvasHeight * (TargetYPosition - 0.5f);

            ContentRectTransform.anchoredPosition = new Vector2(0, startY);
            _canvasGroup.alpha = 0f;

            CreateBaseAnimation(endY)
                .Group(AddCustomAnimations(endY))
                .Chain(CompleteAnimationSequence());
        }

        private Sequence CreateBaseAnimation(float endY)
        {
            return Sequence.Create()
                .Group(
                    Tween.UIAnchoredPositionY(
                        target: ContentRectTransform,
                        endValue: endY,
                        duration: AnimationDuration,
                        ease: Ease.InOutSine)
                )
                .Group(Tween.Alpha(
                    target: _canvasGroup,
                    endValue: 1f,
                    duration: AnimationDuration,
                    ease: Ease.OutCubic
                ))
                .Chain(
                    Tween.Delay(DisplayDuration));
        }

        private Sequence CompleteAnimationSequence()
        {
            Sequence sequence = Sequence.Create()
                .Chain(
                    Tween.Alpha(
                        target: _canvasGroup,
                        endValue: 0f,
                        duration: AnimationDuration,
                        ease: Ease.Linear))
                .OnComplete(
                    this,
                    static popup => popup._onAnimationCompleted?.Invoke());

            return sequence;
        }

        protected virtual Sequence AddCustomAnimations(float endY) =>
            Sequence.Create();
    }
}