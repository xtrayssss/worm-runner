using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours
{
    public sealed class ChapterRow : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _content;

        [SerializeField]
        private Image _lockIconImg;

        public void SetUnlocked(bool isUnlocked)
        {
            _content.gameObject.SetActive(isUnlocked);
            _lockIconImg.gameObject.SetActive(!isUnlocked);
        }

        [Button("Play Unlock Animation", DrawResult = false)]
        public async UniTask PlayUnlockAnimationAsync()
        {
            UIElementTweener.UIElement contentElement = new UIElementTweener.UIElement
            {
                CanvasGroup = _content.GetComponent<CanvasGroup>(),
                RectTransform = _content
            };

            await ShakeLockIcon();

            _content.gameObject.SetActive(true);

            transform.SetAsLastSibling();

            Sequence staggeredPop = UIElementTweener.PopSingle(
                in contentElement,
                animationDuration: 0.6f,
                ease: Ease.OutElastic,
                overshootScale: 2f,
                startAlpha: 0.5f);

            staggeredPop.InsertCallback(
                atTime: staggeredPop.durationTotal * 0.03f,
                target: _lockIconImg,
                static icon => icon.gameObject.SetActive(false));
        }

        private async UniTask ShakeLockIcon()
        {
            Tween shakeRotationTween = Tween.ShakeLocalRotation(
                target: _lockIconImg.rectTransform,
                strength: new Vector3(0, 0, 20f),
                frequency: 15f,
                duration: 1f,
                enableFalloff: false,
                easeBetweenShakes: Ease.Linear
            );

            await shakeRotationTween;
        }
    }
}