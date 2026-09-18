using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

// Предполагается, что PrimeTween подключён и доступен

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    public class MenuButtonAnimator : MonoBehaviour
    {
        private RectTransform rect;
        private Vector2 originalPos;
        private Vector3 originalScale;

        [Header("Jump Settings")]
        public float jumpUpHeight = 40f;
        public float jumpUpDuration = 0.18f;
        public Vector2 peakScale = new Vector2(1.5f, 0.8f);
    
        [Header("Peak Hold")]
        public float peakHoldDuration = 0.1f;
    
        [Header("Fall Settings")]
        public float fallDownHeight = 20f;
        public float fallDownDuration = 0.12f;
        public Vector2 fallScale = new Vector2(0.8f, 1.2f);
    
        [Header("Normalization Settings")]
        public float normalizeDuration = 0.12f;
    
        [Header("Extra Interval")]
        public float extraInterval = 0.18f;
    
        void Awake()
        {
            rect = GetComponent<RectTransform>();
            originalPos = rect.anchoredPosition;
            originalScale = rect.localScale;
        }

        [Button]
        public void Animate()
        {
            var seq = Sequence.Create();

            // Этап 1: Подъём с раздавливанием
            seq.Chain(Tween.UIAnchoredPositionY(rect, originalPos.y + jumpUpHeight, jumpUpDuration, ease: Ease.OutQuad));
            seq.Group(Tween.Scale(rect, new Vector3(peakScale.x, peakScale.y, originalScale.z), jumpUpDuration, ease: Ease.OutQuad));

            // Задержка на пике прыжка
            seq.Chain(Tween.Delay(peakHoldDuration));

            // Этап 2: Падение с эффектом растяжения
            seq.Chain(Tween.UIAnchoredPositionY(rect, originalPos.y - fallDownHeight, fallDownDuration, ease: Ease.InQuad));
            seq.Group(Tween.Scale(rect, new Vector3(fallScale.x, fallScale.y, originalScale.z), fallDownDuration, ease: Ease.InQuad));

            // Этап 3: Нормализация
            seq.Chain(Tween.UIAnchoredPositionY(rect, originalPos.y, normalizeDuration, ease: Ease.OutQuad));
            seq.Group(Tween.Scale(rect, originalScale, normalizeDuration, ease: Ease.OutQuad));

            // Дополнительная задержка в конце анимации
            seq.Chain(Tween.Delay(extraInterval));
        }
    }
}
