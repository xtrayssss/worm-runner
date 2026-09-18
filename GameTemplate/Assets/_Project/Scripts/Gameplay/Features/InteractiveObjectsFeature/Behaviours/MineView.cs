using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using _Project.Scripts.Gameplay.Features.StealthFeature.Services;
using PrimeTween;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class MineView : InteractiveObjectView
    {
        [Header("Mine")]
        [SerializeField]
        private VFXData _explosionVFX;

        [Header("Range Indicator")]
        [SerializeField]
        private MeshFilter _rangeIndicatorMesh;

        [SerializeField]
        private MeshRenderer _rangeIndicatorRenderer;

        [SerializeField]
        private Material _rangeIndicatorMaterial;

        [SerializeField]
        private Color _idleRangeColor = new Color(1f, 0.5f, 0f, 0.3f);

        [SerializeField]
        private Color _armedRangeColor = new Color(1f, 0f, 0f, 0.5f);

        private Material _rangeIndicatorMaterialInstance;
        private CameraService _cameraService;

        public void Construct(CameraService cameraService)
        {
            _cameraService = cameraService;
            _rangeIndicatorMaterialInstance = Instantiate(_rangeIndicatorMaterial);
            _rangeIndicatorRenderer.material = _rangeIndicatorMaterialInstance;
            _rangeIndicatorMaterialInstance.color = _idleRangeColor;
        }

        public void CreateRangeIndicator(float explosionRadius)
        {
            ConeMeshGenerator.UpdateMesh(
                _rangeIndicatorMesh,
                range: explosionRadius,
                angle: 360f,
                precision: 32);
        }

        public void SetArmed(bool armed)
        {
            if (!armed)
                return;

            Tween.MaterialColor(
                _rangeIndicatorMaterialInstance,
                _armedRangeColor,
                duration: 0.2f,
                ease: Ease.OutQuad);
        }

        public void PlayExplosionEffect()
        {
            VFXService.PlayVFX(_explosionVFX, transform.position, scale: 1.7f);
            _cameraService.PlayCameraShake(strength: 0.4f, duration: 0.25f);
        }

        private void OnDestroy() =>
            Destroy(_rangeIndicatorMaterialInstance);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Entity.IsNullOrDisposed())
                return;

            ref readonly MineState mineState = ref Entity.GetComponent<MineState>();

            if (!mineState.ShowDetectionRadius)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, mineState.ExplosionRadius);
        }
#endif
    }
}