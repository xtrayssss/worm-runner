using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TweenFeature
{
    public static class WindowTweener
    {
        public static class WindowDefaults
        {
            private const float DEFAULT_DURATION = 0.3f;

            public static class GrowBounce
            {
                public static TweenSettings<Vector3> ShowScale =>
                    new TweenSettings<Vector3>
                    {
                        startValue = Vector3.zero,
                        endValue = Vector3.one,
                        settings = new TweenSettings
                        {
                            duration = DEFAULT_DURATION,
                            ease = Ease.OutBack
                        }
                    };

                public static TweenSettings<Vector3> HideScale =>
                    new TweenSettings<Vector3>
                    {
                        startValue = Vector3.one,
                        endValue = Vector3.zero,
                        settings = new TweenSettings
                        {
                            duration = DEFAULT_DURATION * 0.8f,
                            ease = Ease.InBack
                        }
                    };

                public static TweenSettings<float> ShowFade =>
                    new TweenSettings<float>
                    {
                        startValue = 0f,
                        endValue = 1f,
                        settings = new TweenSettings
                        {
                            duration = DEFAULT_DURATION,
                            ease = Ease.OutQuad
                        }
                    };

                public static TweenSettings<float> HideFade =>
                    new TweenSettings<float>
                    {
                        startValue = 1f,
                        endValue = 0f,
                        settings = new TweenSettings
                        {
                            duration = DEFAULT_DURATION * 0.8f,
                            ease = Ease.InQuad
                        }
                    };
            }

            public static class ShrinkFade
            {
                public static TweenSettings<Vector3> ShowScale => new TweenSettings<Vector3>
                {
                    startValue = Scale.Value,
                    endValue = Scale.BASE,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION,
                        ease = Ease.OutCubic
                    }
                };

                public static TweenSettings<Vector3> HideScale => new TweenSettings<Vector3>
                {
                    startValue = Scale.BASE,
                    endValue = Scale.Value,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION,
                        ease = Ease.OutCubic
                    }
                };

                public static TweenSettings<float> ShowFade => new TweenSettings<float>
                {
                    startValue = 0f,
                    endValue = 1f,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION,
                        ease = Ease.OutCubic
                    }
                };

                public static TweenSettings<float> HideFade => new TweenSettings<float>
                {
                    startValue = 1f,
                    endValue = 0f,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION,
                        ease = Ease.OutCubic
                    }
                };

                public static class Scale
                {
                    public static readonly Vector3 BASE = Vector3.one;
                    public const float MULTIPLIER = 0.8f;

                    public static Vector3 Value => BASE * MULTIPLIER;
                }
            }

            public static class ZoomPunch
            {
                public static TweenSettings<Vector3> ShowScale => new TweenSettings<Vector3>
                {
                    startValue = Vector3.one * 1.5f,
                    endValue = Vector3.one,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<Vector3> HideScale => new TweenSettings<Vector3>
                {
                    startValue = Vector3.one,
                    endValue = Vector3.one * 1.5f,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION,
                        ease = Ease.InQuad
                    }
                };

                public static TweenSettings<float> ShowFade => new TweenSettings<float>
                {
                    startValue = 0f,
                    endValue = 1f,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION * 0.66f,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<float> HideFade => new TweenSettings<float>
                {
                    startValue = 1f,
                    endValue = 0f,
                    settings = new TweenSettings
                    {
                        duration = DEFAULT_DURATION * 0.66f,
                        ease = Ease.InQuad
                    }
                };
            }

            public static class FadeScaleSlide
            {
                private const float SHOW_DURATION = 0.25f;
                private const float HIDE_DURATION = 0.25f;

                private static readonly Vector3 SLIDE_OFFSET = new Vector3(0, -100, 0);

                private const float START_SCALE = 0.85f;

                public static TweenSettings<Vector3> ShowPosition => new TweenSettings<Vector3>
                {
                    startValue = SLIDE_OFFSET,
                    endValue = Vector3.zero,
                    settings = new TweenSettings
                    {
                        duration = SHOW_DURATION,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<Vector3> HidePosition => new TweenSettings<Vector3>
                {
                    startValue = Vector3.zero,
                    endValue = SLIDE_OFFSET,
                    settings = new TweenSettings
                    {
                        duration = HIDE_DURATION,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<Vector3> ShowScale => new TweenSettings<Vector3>
                {
                    startValue = Vector3.one * START_SCALE,
                    endValue = Vector3.one,
                    settings = new TweenSettings
                    {
                        duration = SHOW_DURATION,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<Vector3> HideScale => new TweenSettings<Vector3>
                {
                    startValue = Vector3.one,
                    endValue = Vector3.one * START_SCALE,
                    settings = new TweenSettings
                    {
                        duration = HIDE_DURATION,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<float> ShowFade => new TweenSettings<float>
                {
                    startValue = 0f,
                    endValue = 1f,
                    settings = new TweenSettings
                    {
                        duration = SHOW_DURATION,
                        ease = Ease.OutQuad
                    }
                };

                public static TweenSettings<float> HideFade => new TweenSettings<float>
                {
                    startValue = 1f,
                    endValue = 0f,
                    settings = new TweenSettings
                    {
                        duration = HIDE_DURATION,
                        ease = Ease.OutQuad
                    }
                };
            }
        }


        public static Sequence ShowFadeScaleSlideWindow(
            RectTransform windowRect,
            CanvasGroup canvasGroup = null,
            bool useUnscaledTime = false)
        {
            Sequence sequence = Sequence.Create(useUnscaledTime: useUnscaledTime);

            sequence.Group(Tween.LocalPosition(
                windowRect,
                WindowDefaults.FadeScaleSlide.ShowPosition));

            sequence.Group(Tween.Scale(
                windowRect,
                WindowDefaults.FadeScaleSlide.ShowScale));

            if (canvasGroup != null)
            {
                sequence.Group(Tween.Alpha(
                    canvasGroup,
                    WindowDefaults.FadeScaleSlide.ShowFade));
            }

            return sequence;
        }

        public static Sequence HideFadeScaleSlideWindow(
            RectTransform windowRect,
            CanvasGroup canvasGroup = null,
            bool useUnscaledTime = false)
        {
            Sequence sequence = Sequence.Create(useUnscaledTime: useUnscaledTime);

            sequence.Group(Tween.LocalPosition(
                windowRect,
                WindowDefaults.FadeScaleSlide.HidePosition));

            sequence.Group(Tween.Scale(
                windowRect,
                WindowDefaults.FadeScaleSlide.HideScale));

            if (canvasGroup != null)
            {
                sequence.Group(Tween.Alpha(
                    canvasGroup,
                    WindowDefaults.FadeScaleSlide.HideFade));
            }

            return sequence;
        }

        public static Sequence ShowShrinkFadeWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup = null,
            Vector3? customBaseScale = null,
            bool useUnscaledTime = false)
        {
            TweenSettings<Vector3> scaleSettings = WindowDefaults.ShrinkFade.ShowScale;

            if (customBaseScale.HasValue)
                scaleSettings = scaleSettings.WithCustomScale(customBaseScale.Value, isShowing: true);

            return AnimateWindow(
                windowTransform,
                canvasGroup,
                scaleSettings,
                WindowDefaults.ShrinkFade.ShowFade,
                useUnscaledTime);
        }

        public static Sequence HideShrinkFadeWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup = null,
            Vector3? customBaseScale = null,
            bool useUnscaledTime = false)
        {
            TweenSettings<Vector3> scaleSettings = WindowDefaults.ShrinkFade.HideScale;

            if (customBaseScale.HasValue)
                scaleSettings = scaleSettings.WithCustomScale(customBaseScale.Value, isShowing: false);

            return AnimateWindow(
                windowTransform,
                canvasGroup,
                scaleSettings,
                WindowDefaults.ShrinkFade.HideFade,
                useUnscaledTime);
        }

        public static Sequence ShowGrowBounceWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup = null,
            TweenSettings<Vector3>? customScaleSettings = null,
            TweenSettings<float>? customFadeSettings = null,
            bool useUnscaledTime = false)
        {
            var scaleSettings = customScaleSettings ?? WindowDefaults.GrowBounce.ShowScale;
            var fadeSettings = customFadeSettings ?? WindowDefaults.GrowBounce.ShowFade;

            return AnimateWindow(windowTransform, canvasGroup, scaleSettings, fadeSettings, useUnscaledTime);
        }

        public static Sequence HideGrowBounceWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup = null,
            TweenSettings<Vector3>? customScaleSettings = null,
            TweenSettings<float>? customFadeSettings = null,
            bool useUnscaledTime = false)
        {
            var scaleSettings = customScaleSettings ?? WindowDefaults.GrowBounce.HideScale;
            var fadeSettings = customFadeSettings ?? WindowDefaults.GrowBounce.HideFade;

            return AnimateWindow(windowTransform, canvasGroup, scaleSettings, fadeSettings, useUnscaledTime);
        }

        public static Sequence ShowZoomPunchWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup = null,
            TweenSettings<Vector3>? customScaleSettings = null,
            TweenSettings<float>? customFadeSettings = null,
            bool useUnscaledTime = false)
        {
            var scaleSettings = customScaleSettings ?? WindowDefaults.ZoomPunch.ShowScale;
            var fadeSettings = customFadeSettings ?? WindowDefaults.ZoomPunch.ShowFade;

            return AnimateWindow(windowTransform, canvasGroup, scaleSettings, fadeSettings, useUnscaledTime);
        }

        public static Sequence HideZoomPunchWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup = null,
            TweenSettings<Vector3>? customScaleSettings = null,
            TweenSettings<float>? customFadeSettings = null,
            bool useUnscaledTime = false)
        {
            var scaleSettings = customScaleSettings ?? WindowDefaults.ZoomPunch.HideScale;
            var fadeSettings = customFadeSettings ?? WindowDefaults.ZoomPunch.HideFade;

            return AnimateWindow(windowTransform, canvasGroup, scaleSettings, fadeSettings, useUnscaledTime);
        }

        private static Sequence AnimateWindow(
            Transform windowTransform,
            CanvasGroup canvasGroup,
            TweenSettings<Vector3> scaleSettings,
            TweenSettings<float> fadeSettings,
            bool useUnscaledTime)
        {
            Sequence sequence = Sequence.Create(useUnscaledTime: useUnscaledTime);

            sequence.Group(Tween.Scale(windowTransform, scaleSettings));

            if (canvasGroup != null)
                sequence.Group(Tween.Alpha(canvasGroup, fadeSettings));

            return sequence;
        }
    }
}