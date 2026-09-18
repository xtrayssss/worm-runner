using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public class InteractiveObjectView : EntityView
    {
        [SerializeField]
        private CommonInteractiveObjectView _commonView;

        [SerializeField]
        private float _destructionDuration = 0.12f;

        public VFXData DestructionVFX => _commonView.DestructionVFX;
        public HealthBar HealthBar => _commonView.HealthBar;
        public Collider Collider => _commonView.Collider;
        public VFXData BuffVfx => _commonView.BuffVfx;
        public Transform VisualRoot => _commonView.VisualRoot;

        protected Vector3 OriginalScale = Vector3.one;
        private Tween _hitAnimation;
        protected VFXService VFXService { get; private set; }

        public void Construct(VFXService vfxService, Vector3 originalScale)
        {
            VFXService = vfxService;
            OriginalScale = originalScale;
            HealthBar?.Construct();
        }

        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        public Tween PlayHitAnimation()
        {
            if (_hitAnimation.isAlive)
                _hitAnimation.Stop();

            const float PUNCH_DURATION = 0.13f;
            const float PUNCH_SCALE_PERCENT = 0.08f;

            transform.localScale = OriginalScale;

            _hitAnimation = Tween.PunchScale(
                target: transform,
                strength: OriginalScale * PUNCH_SCALE_PERCENT,
                duration: PUNCH_DURATION,
                easeBetweenShakes: Ease.Linear,
                frequency: 6,
                enableFalloff: false
            );

            return default;
        }

        public Tween PlayDestructionAnimation()
        {
            return Tween.Scale(
                transform,
                Vector3.zero,
                duration: _destructionDuration,
                ease: Ease.InBack);
        }

        public virtual InteractiveObjectView PlayDestructionVFX(float? scale = null)
        {
            VFXService.PlayVFX(DestructionVFX, transform.position, scale: scale);

            return this;
        }

#if UNITY_EDITOR
        [Button("Play Destruction Animation")]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        public Tween PlayDestructionAnimationEditor()
        {
            transform.localScale = OriginalScale;
            return PlayDestructionAnimation();
        }
#endif
    }
}