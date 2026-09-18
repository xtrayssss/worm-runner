using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class EnemyView : InteractiveObjectView
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private Transform _shadow;

        [SerializeField]
        private CharacterAnimator _characterAnimator;

        [SerializeField]
        private Transform _shootPoint;

        [SerializeField]
        private VFXData _shootVFX;

        [FormerlySerializedAs("_explosionVFX")]
        [SerializeField]
        [BoxGroup("Kamikaze Enemy")]
        private VFXData _kamikazeExplosionVFX;

        public Transform ShootPoint => _shootPoint;

        private VFXService _vfxService;
        private CameraService _cameraService;

        public void Construct(VFXService vfxService, CameraService cameraService)
        {
            _cameraService = cameraService;
            _characterAnimator.Construct(VisualRoot, _spriteRenderer, shadow: _shadow);
            _vfxService = vfxService;
        }

        public void StartJump() =>
            _characterAnimator.StartJump();

        public void StopJump() =>
            _characterAnimator.StopJump();

        public void StartIdle() =>
            _characterAnimator.StartIdle();

        public void StopIdle() =>
            _characterAnimator.StopIdle();

        public void PlayShootEffect()
        {
            if (_shootVFX != null)
                _vfxService.PlayVFX(_shootVFX, transform.position);
        }

        public void PlayKamikazeExplosionEffect()
        {
            _vfxService.PlayVFX(_kamikazeExplosionVFX, transform.position, scale: 1.4f);
            _cameraService.PlayCameraShake(strength: 0.4f, duration: 0.2f);
        }

        public void PlayDestructionVFX() =>
            _vfxService.PlayVFX(DestructionVFX, transform.position, scale: 1.2f);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Entity.IsNullOrDisposed())
                return;

            if (!Entity.Has<KamikazeEnemy>())
                return;

            ref readonly KamikazeEnemy kamikazeEnemy = ref Entity.GetComponent<KamikazeEnemy>();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + kamikazeEnemy.ExplosionOffset, kamikazeEnemy.ExplosionRadius);
        }
#endif
    }
}