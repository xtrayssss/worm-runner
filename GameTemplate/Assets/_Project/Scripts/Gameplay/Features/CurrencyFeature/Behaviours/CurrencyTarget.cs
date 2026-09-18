using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;
using Tween = PrimeTween.Tween;

namespace _Project.Scripts.Gameplay.Features.CurrencyFeature.Behaviours
{
    public sealed class CurrencyTarget : MonoBehaviour
    {
        [FormerlySerializedAs("animationScale")] [SerializeField]
        private float _animationScale = 1.2f;

        [FormerlySerializedAs("animationDuration")] [SerializeField]
        private float _animationDuration = 0.4f;

        private Tween _scale;

        public void AnimateCurrencyTarget()
        {
            if (!_scale.isAlive)
            {
                _scale = Tween.Scale(
                    target: transform,
                    endValue: _animationScale,
                    duration: _animationDuration,
                    cycles: 2,
                    cycleMode: CycleMode.Yoyo,
                    ease: Ease.OutSine);
            }
        }
    }
}