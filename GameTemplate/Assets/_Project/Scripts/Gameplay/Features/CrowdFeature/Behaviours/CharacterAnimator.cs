using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using Sequence = PrimeTween.Sequence;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public sealed class CharacterAnimator : MonoBehaviour
    {
        private JumpAnimation _jumpAnimation;
        private Sequence _idleSequence;
        private Vector3 _visualRootInitialLocalPosition;
        private Transform _visualRoot;
        private SpriteRenderer _spriteRenderer;
        private NestedFadeGroup.NestedFadeGroup _fadeGroup;
        private Vector3 _shadowInitialLocalScale;
        private Transform _shadow;

        private struct JumpAnimation
        {
            public Sequence ShadowSequence;
            public Sequence JumpSequence;
            public Sequence SquashSequence;

            public void Stop()
            {
                if (JumpSequence.isAlive)
                    JumpSequence.Stop();

                if (ShadowSequence.isAlive)
                    ShadowSequence.Stop();

                if (SquashSequence.isAlive)
                    SquashSequence.Stop();
            }
        }

        public void Construct(
            Transform visualRoot,
            SpriteRenderer spriteRenderer,
            [CanBeNull] NestedFadeGroup.NestedFadeGroup canvasGroup = null,
            [CanBeNull] Transform shadow = null)
        {
            _spriteRenderer = spriteRenderer;
            _visualRoot = visualRoot;
            _visualRootInitialLocalPosition = _visualRoot.localPosition;

            _shadow = shadow;

            if (_shadow != null)
                _shadowInitialLocalScale = _shadow.localScale;

            _fadeGroup = canvasGroup;
        }

        private void OnDestroy()
        {
            _jumpAnimation.Stop();

            if (_idleSequence.isAlive)
                _idleSequence.Stop();
        }

        public void StartIdle()
        {
            const float IDLE_SCALE_AMOUNT = 0.05f;
            const float IDLE_DURATION = 1.9f;
            const Ease IDLE_EASE = Ease.InOutSine;
            const float IDLE_Y_MOVEMENT = 0.08f;

            StopIdle();

            _idleSequence = Sequence.Create(cycles: -1, cycleMode: CycleMode.Yoyo)
                .Group(Tween.Scale(
                    _visualRoot,
                    Vector3.one * (1f + IDLE_SCALE_AMOUNT),
                    IDLE_DURATION / 2f,
                    IDLE_EASE))
                .Group(Tween.LocalPositionY(
                    _visualRoot,
                    _visualRoot.localPosition.y + IDLE_Y_MOVEMENT,
                    IDLE_DURATION / 2f,
                    IDLE_EASE));
        }

        public void StopIdle()
        {
            if (_idleSequence.isAlive)
            {
                _idleSequence.Stop();
                _visualRoot.localScale = Vector3.one;
                _visualRoot.localPosition = _visualRootInitialLocalPosition;
            }
        }

        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        public void StartJump()
        {
            _jumpAnimation = new JumpAnimation();

            const float DURATION = 0.5f;
            const float STRETCH_SCALE = 0.87f;
            const float SQUASH_SCALE = 1.13f;
            const float JUMP_HEIGHT = 0.75f;
            const float JUMP_DURATION = DURATION / 2f;

            StopJump();

            Sequence squashSequence = Sequence.Create(cycles: -1, cycleMode: CycleMode.Yoyo)
                .Chain(Tween.Scale(
                    _visualRoot,
                    new Vector3(STRETCH_SCALE, SQUASH_SCALE, 1f),
                    JUMP_DURATION,
                    Ease.OutQuad));

            _jumpAnimation.SquashSequence = squashSequence;

            Sequence jumpSequence = Sequence.Create(cycles: -1, cycleMode: CycleMode.Yoyo)
                .Chain(Tween.LocalPositionY(
                    target: _visualRoot,
                    endValue: _visualRootInitialLocalPosition.y + JUMP_HEIGHT,
                    duration: JUMP_DURATION,
                    ease: Ease.OutQuad));

            _jumpAnimation.JumpSequence = jumpSequence;

            if (_shadow != null)
            {
                var shadowSequence = Sequence.Create(cycles: -1, CycleMode.Yoyo)
                    .Chain(Tween.Scale(
                        _shadow,
                        _shadow.localScale * 0.85f,
                        JUMP_DURATION,
                        Ease.Linear));

                _jumpAnimation.ShadowSequence = shadowSequence;
            }
        }

        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        public void StopJump()
        {
            _jumpAnimation.Stop();

            _visualRoot.localScale = Vector3.one;
            _visualRoot.localPosition = _visualRootInitialLocalPosition;

            if (_shadow != null)
                _shadow.localScale = _shadowInitialLocalScale;
        }

        public void StopAllAnimations()
        {
            StopJump();
            StopIdle();
        }

        public UniTask PlayDissolveEffectAsync()
        {
            const float DISSOLVE_DURATION = 0.8f;
            const string DISSOLVE_PROPERTY = "_DissolveAmount";

            if (_spriteRenderer == null)
                return UniTask.CompletedTask;

            Material dissolveMaterial = _spriteRenderer.material;
            dissolveMaterial.SetFloat(DISSOLVE_PROPERTY, 0f);

            const Ease EASE = Ease.InQuad;

            Tween spriteDissolveTween = Tween.Custom(
                dissolveMaterial,
                startValue: 0f,
                endValue: 1f,
                duration: DISSOLVE_DURATION,
                onValueChange: static (mat, value) => mat.SetFloat(DISSOLVE_PROPERTY, value),
                ease: EASE
            );

            Tween fadeGroupTween = Tween.Custom(
                _fadeGroup,
                startValue: 1f,
                endValue: 0f,
                duration: DISSOLVE_DURATION * 0.6f,
                ease: EASE,
                onValueChange: static (group, value) => group.AlphaSelf = value
            );

            return UniTask.WhenAll(
                spriteDissolveTween.ToYieldInstruction().ToUniTask(),
                fadeGroupTween.ToYieldInstruction().ToUniTask());
        }

#if UNITY_EDITOR
        [Button("Test Idle")]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        [PropertySpace(spaceBefore: 5)]
        public void TestIdleAnimation()
        {
            if (_idleSequence.isAlive)
                StopIdle();
            else
                StartIdle();
        }
#endif
    }
}