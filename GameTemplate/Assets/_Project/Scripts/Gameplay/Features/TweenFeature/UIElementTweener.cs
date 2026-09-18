using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TweenFeature
{
    public static class UIElementTweener
    {
        public struct UIElement
        {
            public RectTransform RectTransform;
            public CanvasGroup CanvasGroup;
        }

        public static class StaggeredPopDefaults
        {
            public const float DEFAULT_DURATION = 0.17f;
            public const float OVERSHOOT_SCALE = 1.5f;
        }

        public static Sequence StaggeredPop(
            UIElement[] elements,
            float animationDuration = StaggeredPopDefaults.DEFAULT_DURATION,
            float overshootScale = StaggeredPopDefaults.OVERSHOOT_SCALE,
            bool useUnscaledTime = false,
            Ease ease = Ease.OutQuad)
        {
            Sequence sequence = Sequence.Create(useUnscaledTime: useUnscaledTime);

            foreach (ref readonly UIElement element in elements.AsSpan())
            {
                Sequence elementSequence = PopSingle(
                    in element,
                    animationDuration,
                    overshootScale,
                    useUnscaledTime, 
                    ease);
                
                sequence.Chain(elementSequence);
            }

            return sequence;
        }

        public static Sequence PopSingle(
            in UIElement element,
            float animationDuration = StaggeredPopDefaults.DEFAULT_DURATION,
            float overshootScale = StaggeredPopDefaults.OVERSHOOT_SCALE,
            bool useUnscaledTime = false,
            Ease ease = Ease.OutQuad,
            float startAlpha = 0f)
        {
            Vector3 originalScale = element.RectTransform.localScale;

            element.CanvasGroup.alpha = startAlpha;
            element.RectTransform.localScale *= overshootScale;

            Sequence elementSequence = Sequence.Create(useUnscaledTime: useUnscaledTime);

            elementSequence
                .Group(Tween.Scale(
                    element.RectTransform,
                    originalScale,
                    duration: animationDuration,
                    ease))
                .Group(Tween.Alpha(
                    element.CanvasGroup,
                    1f,
                    duration: animationDuration,
                    ease));

            return elementSequence;
        }
    }
}