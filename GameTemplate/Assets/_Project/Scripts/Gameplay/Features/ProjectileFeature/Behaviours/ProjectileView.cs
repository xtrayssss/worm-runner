using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Sequence = PrimeTween.Sequence;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours
{
    public sealed class ProjectileView : EntityView
    {
        [SerializeField]
        private Rigidbody _rigidbody;

        [SerializeField]
        private GameObject _visualRoot;

        [FormerlySerializedAs("_explosionVFX")]
        [SerializeField]
        private VFXData _impactVFX;

        private Sequence _jumpSequence;
        private VFXService _vfxService;
        public GameObject VisualRoot => _visualRoot;
        public VFXData ImpactVFX => _impactVFX;
        
        public void Construct(VFXService vfxService) => 
            _vfxService = vfxService;

        [Button]
        [HideIf("@_Project.Scripts.Gameplay.Features.CommonFeature.EditorTools.EditorUtils.IsPrefab(this.gameObject)")]
        public void StartAnimation()
        {
            const float DURATION = 0.31f;

            const double STRETCH_SCALE = 0.85 * 0.6f;
            const double SQUASH_SCALE = 1.19 * 1.5f;

            const double STRETCH_SCALE2 = 1.19 * 1.3f;
            const double SQUASH_SCALE2 = 0.85 * 0.7f;

            const int JUMP_HEIGHT = 1;

            const float JUMP_DURATION = DURATION / 2f;
            _jumpSequence = Sequence.Create(cycles: -1);
            float iniPosY = transform.position.y;

            if (_jumpSequence.isAlive)
                _jumpSequence.Stop();

            transform.localScale = new Vector3(
                (float)STRETCH_SCALE,
                (float)SQUASH_SCALE,
                1);

            _jumpSequence = Sequence.Create();

            Sequence squashSequence = Sequence.Create()
                .Chain(Tween.Scale(transform,
                    Vector3.one,
                    DURATION,
                    Ease.OutCubic));

            _jumpSequence.Chain(squashSequence);
        }
        
        public Rigidbody Rigidbody => _rigidbody;

        public void PlayExplosionVFX(VFXData explosionExplosionVFX, Vector3 requestPoint)
        {
            _vfxService.PlayVFX(explosionExplosionVFX, requestPoint);
        }
    }
}