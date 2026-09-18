using PrimeTween;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Behaviours
{
    public sealed class DamagePopupView : MonoBehaviour
    {
        public TextMeshPro Text { get; private set; }

        public void Construct() =>
            Text = GetComponentInChildren<TextMeshPro>();

        public void Show(float duration)
        {
            SetTextAlpha(1f);

            Sequence sequence = Sequence.Create();

            sequence
                .Group(
                    Tween.LocalPositionY(
                        target: transform,
                        endValue: transform.localPosition.y + 2f,
                        duration: duration,
                        ease: Ease.OutCubic))
                .Group(
                    Tween.Alpha(
                        target: Text,
                        endValue: 0f,
                        duration: duration * 0.7f,
                        ease: Ease.InCubic))
                .OnComplete(
                    this,
                    static view =>
                    {
                        if (view != null)
                            Destroy(view.gameObject);
                    },
                    warnIfTargetDestroyed: false);
        }

        private void SetTextAlpha(float alpha)
        {
            Color color = Text.color;
            color.a = alpha;
            Text.color = color;
        }
    }
}