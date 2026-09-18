using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class CollectibleView : InteractiveObjectView
    {
        [SerializeField]
        private Collider _collider;

        [Header("Animation")]
        [SerializeField]
        private float _floatAmplitude = 0.3f;

        [SerializeField]
        private float _floatSpeed = 2f;

        [SerializeField]
        private float _rotationSpeed = 50f;

        private Vector3 _initialPosition;
        private float _timeOffset;
        private Tween _floatingAnimation;

        public Tween PlayFloatingAnimation()
        {
            const float FLOAT_DURATION = 1f;
            const float FLOAT_AMOUNT = 0.3f;

            _floatingAnimation = Tween.LocalPositionY(
                VisualRoot,
                VisualRoot.localPosition.y + FLOAT_AMOUNT,
                FLOAT_DURATION,
                ease: Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
            );

            return _floatingAnimation;
        }

        public override InteractiveObjectView PlayDestructionVFX(float? scale = null)
        {
            _floatingAnimation.Stop();

            return base.PlayDestructionVFX(scale);
        }
    }
}