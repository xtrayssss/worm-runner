using _Project.Scripts.Gameplay.Features.CheatsFeature;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours
{
    public class CrowdMemberView : EntityView
    {
        [SerializeField]
        private CommonCrowdMemberView _commonView;

        [SerializeField]
        [CanBeNull]
        private Transform _shadow;

        public Transform VisualRoot => _commonView.VisualRoot;
        public SpriteRenderer SpriteRenderer => _commonView.SpriteRenderer;
        public Collider Collider => _commonView.Collider;
        public Rigidbody Rigidbody => _commonView.Rigidbody;

        public virtual void Construct() =>
            _commonView.Animator.Construct(VisualRoot, _commonView.SpriteRenderer, _commonView.FadeGroup, _shadow);

        public void StartIdle() =>
            _commonView.Animator.StartIdle();

        public void StopIdle() =>
            _commonView.Animator.StopIdle();

        public void StartJump() =>
            _commonView.Animator.StartJump();

        public void StopJump() =>
            _commonView.Animator.StopJump();

        public void StopAllAnimations() =>
            _commonView.Animator.StopAllAnimations();

        public UniTask PlayDissolveEffectAsync() =>
            _commonView.Animator.PlayDissolveEffectAsync();

#if UNITY_EDITOR
        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        [GUIColor(0.8f, 0.2f, 0.2f)]
        [PropertySpace(spaceBefore: 5)]
        public void Kill() =>
            Cheats.RemoveSpecificCrowdMembers(new[] { Entity });

        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        [PropertySpace(spaceBefore: 5)]
        public void TestIdleAnimation()
        {
            if (_commonView.Animator == null)
                return;

            _commonView.Animator.TestIdleAnimation();
        }
#endif
    }
}