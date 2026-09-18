using PrimeTween;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public sealed class DecorativeMemberView : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private CharacterAnimator _characterAnimator;

        [SerializeField]
        private Transform _visualRoot;

        private Sequence _idleSequence;
        private Tween _punchAnimation;
        private Vector3 _originalScale;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        public void Construct()
        {
            _originalScale = transform.localScale;
            _characterAnimator.Construct(_visualRoot, _spriteRenderer);
        }

        public void StartIdle() =>
            _characterAnimator.StartIdle();

        private void StopIdle() =>
            _characterAnimator.StopIdle();

        public Tween PlayPunchAnimation()
        {
            if (_punchAnimation.isAlive)
                _punchAnimation.Stop();

            const float PUNCH_DURATION = 0.2f;
            const float PUNCH_SCALE_PERCENT = 0.08f;

            transform.localScale = _originalScale;

            _punchAnimation = Tween.PunchScale(
                target: transform,
                strength: _originalScale * PUNCH_SCALE_PERCENT,
                duration: PUNCH_DURATION,
                easeBetweenShakes: Ease.Linear,
                frequency: 6,
                enableFalloff: false
            );

            return default;
        }
    }
}