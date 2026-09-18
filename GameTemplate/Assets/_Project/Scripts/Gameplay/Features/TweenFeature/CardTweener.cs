using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TweenFeature
{
    public static class CardTweener
    {
        public class Card
        {
            public RectTransform RectTransform;
            public CanvasGroup CanvasGroup;
        }

        public static class CardDefaults
        {
            private const float DEFAULT_DURATION = 0.7f;
            private const float DEFAULT_DELAY_BETWEEN_CARDS = 0.3f;

            public static float AnimationDuration => DEFAULT_DURATION;
            public static float DelayBetweenCards => DEFAULT_DELAY_BETWEEN_CARDS;
        }

        public static Sequence ShowSingleCard(
            Card card,
            float animationDuration = 0.7f,
            bool useUnscaledTime = false)
        {
            CanvasGroup canvasGroup = card.CanvasGroup ?? card.RectTransform.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = card.RectTransform.gameObject.AddComponent<CanvasGroup>();

            card.RectTransform.localScale = Vector3.one * 0.5f;
            canvasGroup.alpha = 0;

            return Sequence.Create(useUnscaledTime: useUnscaledTime)
                .Group(Tween.Scale(card.RectTransform, Vector3.one * 1.2f, duration: animationDuration / 2f,
                    ease: Ease.OutBack))
                .Group(Tween.Alpha(canvasGroup, 1f, duration: animationDuration / 2f))
                .Chain(Tween.Scale(card.RectTransform, Vector3.one, duration: animationDuration / 2f));
        }

        public static void ShowCards(
            List<Card> cards,
            float animationDuration = 0.7f,
            float delayBetweenCards = 0.3f,
            float customScale = 1f,
            bool useUnscaledTime = false)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];

                CanvasGroup canvasGroup = card.CanvasGroup ?? card.RectTransform.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                    canvasGroup = card.RectTransform.gameObject.AddComponent<CanvasGroup>();

                card.RectTransform.localScale = Vector3.one * (customScale * 0.5f);
                canvasGroup.alpha = 0;

                Sequence.Create(useUnscaledTime: useUnscaledTime)
                    .Insert(
                        atTime: i * delayBetweenCards,
                        tween: Tween.Scale(card.RectTransform, customScale * 1.2f, duration: animationDuration / 2f,
                            ease: Ease.OutBack)
                    )
                    .Insert(
                        atTime: i * delayBetweenCards,
                        tween: Tween.Alpha(canvasGroup, 1f, duration: animationDuration / 2f)
                    )
                    .Chain(
                        Tween.Scale(card.RectTransform, customScale, duration: animationDuration / 2f)
                    );
            }
        }
    }
}