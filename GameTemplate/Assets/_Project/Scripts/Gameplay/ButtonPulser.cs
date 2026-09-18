using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class ButtonPulser : MonoBehaviour
    {
        [SerializeField] private float _pulseScale = 1.1f;
        [SerializeField] private float _pulseDuration = 1.5f;
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private bool _playOnStart = true;

        private Vector3 _originalScale;
        private Tween _pulseAnimation;
        public Tween PulseAnimation => _pulseAnimation;

        public void Construct() => 
            _originalScale = transform.localScale;

        public void StopPulse()
        {
            _pulseAnimation.Stop();
            transform.localScale = _originalScale;
        }

        [Button]
        public void PlayPulse()
        {
            StopPulse();

            _pulseAnimation = Tween.Scale(
                target: transform,
                endValue: _originalScale * _pulseScale,
                duration: _pulseDuration,
                ease: _ease,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );
        }
    }
}