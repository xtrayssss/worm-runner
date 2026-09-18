using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.TweenFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class MortarTelegraphView : EntityView
    {
        [SerializeField]
        private SpriteRenderer _telegraphCircle;

        [SerializeField]
        private VFXData _impactEffect;

        [SerializeField]
        private AudioSource _impactSound;

        [SerializeField]
        private ProjectileView _projectilePrefab;

        private VFXService _vfxService;
        private Sequence _pulseTween;
        private CameraService _cameraService;
        private AudioService _audioService;

        public void Construct(VFXService vfxService, CameraService cameraService, AudioService audioService)
        {
            _cameraService = cameraService;
            _vfxService = vfxService;
            _audioService = audioService;
        }

        [Button]
        public void SetRadius(float radius) =>
            _telegraphCircle.transform.localScale = new Vector3(5.2f, 5.2f, 5.2f);

        public Tween PlayDestructionAnimation()
        {
            const float DURATION = 0.12f;

            _pulseTween.Stop();

            return Tween.Scale(
                transform,
                Vector3.zero,
                duration: DURATION,
                ease: Ease.InBack);
        }

        public Sequence LaunchProjectile(float duration)
        {
            const float PROJECTILE_HEIGHT = 0f;
            const float PROJECTILE_ARC_HEIGHT = 12f;

            float randomX = Random.Range(0, 2) == 0 ? 0f : Screen.width;

            Vector3 screenTopLeft = new Vector3(randomX, Screen.height, 45f);

            Vector3 startPosition = _cameraService.MainCamera.ScreenToWorldPoint(screenTopLeft);

            startPosition.y = PROJECTILE_HEIGHT;
            startPosition.z = transform.position.z;

            ref readonly MortarStrikeState strikeState = ref Entity.GetComponent<MortarStrikeState>();

            Vector3 randomOffset = new Vector3(
                Random.Range(-0.2f, 0.2f) * strikeState.ImpactRadius,
                0,
                Random.Range(-0.2f, 0.2f) * strikeState.ImpactRadius);

            Vector3 targetPosition = transform.position + randomOffset;

            ProjectileView projectile =
                Instantiate(_projectilePrefab, startPosition, _projectilePrefab.transform.rotation);

            _audioService.PlaySound(AudioId.Sfx.Gameplay.MORTAR_LAUNCH);

            Vector3 previousPosition = projectile.transform.position;

            Sequence sequence = TweenExtensions
                .Jump(
                    projectile.transform,
                    targetPosition,
                    duration,
                    PROJECTILE_ARC_HEIGHT)
                .Group(Tween.Custom(
                    startValue: 0f,
                    endValue: 1f,
                    duration: duration,
                    onValueChange: _ =>
                    {
                        Vector3 currentPos = projectile.transform.position;
                        Vector3 velocity = (currentPos - previousPosition) / Time.deltaTime;
                        previousPosition = currentPos;

                        if (velocity.sqrMagnitude > 0.001f)
                        {
                            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
                        }
                    }
                ))
                .OnComplete(projectile, static p => Destroy(p.gameObject));

            return sequence;
        }

        public void PlayImpactEffect()
        {
            _vfxService.PlayVFX(_impactEffect, transform.position, scale: 1.5f);
            _cameraService.PlayCameraShake(strength: 0.8f, duration: 0.2f);
        }

        public void PlayPulseAnimation()
        {
            ref readonly MortarStrikeState strikeState = ref Entity.GetComponent<MortarStrikeState>();

            _pulseTween = Sequence.Create()
                .Group(Tween.Alpha(
                    target: _telegraphCircle,
                    endValue: 0.2f,
                    duration: strikeState.TelegraphDuration,
                    ease: Ease.InOutSine));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Entity.IsNullOrDisposed())
                return;

            float radius = Entity.GetComponent<MortarStrikeState>().ImpactRadius;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}