using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TweenFeature
{
    public static class SettingsExtensions
    {
        public static TweenSettings WithDuration(this TweenSettings settings, float duration)
        {
            var newSettings = settings;
            newSettings.duration = duration;
            return newSettings;
        }

        public static TweenSettings WithEase(this TweenSettings settings, Ease ease)
        {
            var newSettings = settings;
            newSettings.ease = ease;
            return newSettings;
        }

        public static TweenSettings WithUnscaledTime(this TweenSettings settings, bool useUnscaledTime = true)
        {
            var newSettings = settings;
            newSettings.useUnscaledTime = useUnscaledTime;
            return newSettings;
        }

        public static TweenSettings<T> WithStartValue<T>(this TweenSettings<T> settings, T startValue)
            where T : struct
        {
            var newSettings = settings;
            newSettings.startValue = startValue;
            return newSettings;
        }

        public static TweenSettings<T> WithEndValue<T>(this TweenSettings<T> settings, T endValue) where T : struct
        {
            var newSettings = settings;
            newSettings.endValue = endValue;
            return newSettings;
        }

        public static TweenSettings<Vector3> WithCustomScale(
            this TweenSettings<Vector3> settings,
            Vector3 baseScale,
            bool isShowing)
        {
            if (isShowing)
            {
                return settings
                    .WithStartValue(baseScale * WindowTweener.WindowDefaults.ShrinkFade.Scale.MULTIPLIER)
                    .WithEndValue(baseScale);
            }

            return settings
                .WithStartValue(baseScale)
                .WithEndValue(baseScale * WindowTweener.WindowDefaults.ShrinkFade.Scale.MULTIPLIER);
        }
    }
}