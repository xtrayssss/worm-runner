using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.TweenFeature
{
    public static class ButtonTweener
    {
        public static class ButtonDefaults
        {
            public const float DEFAULT_DURATION = 0.25f;

            public static class Punch
            {
                public static ShakeSettings PressScale =>
                    new ShakeSettings
                    {
                        duration = DEFAULT_DURATION,
                        strength = new Vector3(0.5f, 0.5f, 0.5f),
                        frequency = 1,
                        easeBetweenShakes = Ease.OutQuart,
                        enableFalloff = false,
                        asymmetry = 0
                    };
            }

            public static class ErrorShake
            {
                private const float ERROR_DURATION = 1.5f;

                public static ShakeSettings DefaultShake =>
                    new ShakeSettings
                    {
                        strength = new Vector3(30f, 0f, 0f),
                        duration = ERROR_DURATION,
                        frequency = 10,
                        easeBetweenShakes = Ease.Linear,
                        enableFalloff = true,
                        asymmetry = 0f
                    };

                public static ShakeSettings VerticalShake =>
                    new ShakeSettings
                    {
                        strength = new Vector3(0f, 30f, 0f),
                        duration = ERROR_DURATION,
                        frequency = 10,
                        easeBetweenShakes = Ease.Linear,
                        enableFalloff = true,
                        asymmetry = 0f
                    };

                public static ShakeSettings StrongShake =>
                    new ShakeSettings
                    {
                        strength = new Vector3(30f, 30f, 0f),
                        duration = ERROR_DURATION,
                        frequency = 12,
                        easeBetweenShakes = Ease.Linear,
                        enableFalloff = true,
                        asymmetry = 0f
                    };
            }
        }

        public static Tween AnimateButtonPunch(
            Transform buttonTransform,
            ShakeSettings? customPressSettings = null,
            bool useUnscaledTime = false)
        {
            var settings = customPressSettings ?? ButtonDefaults.Punch.PressScale;

            return Tween.PunchScale(buttonTransform, settings);
        }

        public static Tween AnimateErrorShake(
            Transform buttonTransform,
            ShakeSettings? customShakeSettings = null,
            bool useUnscaledTime = false)
        {
            ShakeSettings settings = customShakeSettings ?? ButtonDefaults.ErrorShake.DefaultShake;

            return Tween.PunchLocalPosition(
                target: buttonTransform,
                settings: settings);
        }

        public static Tween AnimateVerticalErrorShake(
            Transform buttonTransform,
            bool useUnscaledTime = false)
        {
            return AnimateErrorShake(
                buttonTransform,
                ButtonDefaults.ErrorShake.VerticalShake,
                useUnscaledTime);
        }

        public static Tween AnimateStrongErrorShake(
            Transform buttonTransform,
            bool useUnscaledTime = false)
        {
            return AnimateErrorShake(
                buttonTransform,
                ButtonDefaults.ErrorShake.StrongShake,
                useUnscaledTime);
        }

        public static Tween AnimateButtonPressScale(
            Transform buttonTransform,
            Vector3? customScale = null,
            float? customDuration = null,
            bool useUnscaledTime = false)
        {
            Vector3 targetScale = customScale ?? Vector3.one * 1.1f;
            float duration = customDuration ?? ButtonDefaults.DEFAULT_DURATION;

            return Tween.Scale(
                buttonTransform,
                targetScale,
                duration,
                Ease.OutQuart,
                useUnscaledTime: useUnscaledTime);
        }

        public static Tween AnimateButtonReleaseScale(
            Transform buttonTransform,
            Vector3? customScale = null,
            float? customDuration = null,
            bool useUnscaledTime = false)
        {
            Vector3 targetScale = customScale ?? Vector3.one;
            float duration = customDuration ?? ButtonDefaults.DEFAULT_DURATION;

            return Tween.Scale(
                buttonTransform,
                targetScale,
                duration,
                Ease.OutQuart,
                useUnscaledTime: useUnscaledTime);
        }
    }
}