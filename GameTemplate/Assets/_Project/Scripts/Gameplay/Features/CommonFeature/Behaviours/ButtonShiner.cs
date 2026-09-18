using Coffee.UIExtensions;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Behaviours
{
    public sealed class ButtonShiner : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField, ReadOnly] private ShinyEffectForUGUI _shineEffect;

        [Header("Settings")]
        [SerializeField] private float _shineDuration = 1f;

        [SerializeField] private Ease _shineEase = Ease.InOutSine;
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _loop = true;
        [SerializeField] private float _delayBetweenShines = 0.5f;

        private Tween _animation;

        private void OnEnable()
        {
            _shineEffect = GetComponentInChildren<ShinyEffectForUGUI>();

            if (_playOnEnable)
                Play();
        }

        private void OnDisable() =>
            Stop();

        private void Play()
        {
            Stop();

            _shineEffect.location = -_shineEffect.width;

            _animation = Tween.Custom(
                target: _shineEffect,
                startValue: 0,
                endValue: 1f,
                duration: _shineDuration,
                onValueChange: static (effect, value) => effect.location = value,
                ease: _shineEase,
                cycles: _loop ? -1 : 1,
                endDelay: _delayBetweenShines);
        }

        private void Stop() =>
            _animation.Stop();

        public void PlayOnce()
        {
            bool originalLoop = _loop;
            _loop = false;
            Play();
            _loop = originalLoop;
        }
    }
}

