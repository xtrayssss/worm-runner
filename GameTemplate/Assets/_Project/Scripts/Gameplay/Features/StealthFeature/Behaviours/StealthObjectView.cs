using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StealthFeature.Services;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours
{
    public class StealthObjectView : InteractiveObjectView
    {
        [SerializeField]
        [PropertyOrder(-1)]
        private CommonStealthObjectView _commonStealthObjectView;

        public CommonStealthObjectView CommonStealthObjectView => _commonStealthObjectView;

        protected virtual void Awake()
        {
            if (_commonStealthObjectView.VisionConeRenderer != null &&
                _commonStealthObjectView.VisionConeMaterial != null)
            {
                _commonStealthObjectView.InstanceMaterial = Instantiate(_commonStealthObjectView.VisionConeMaterial);
                _commonStealthObjectView.VisionConeRenderer.material = _commonStealthObjectView.InstanceMaterial;
                _commonStealthObjectView.InstanceMaterial.color = _commonStealthObjectView.NormalColor;
            }
        }

        [Button]
        public virtual void UpdateVisionCone(float range, float angle,
            ConeMeshGenerator.ConeDirection direction = ConeMeshGenerator.ConeDirection.FORWARD)
        {
            if (_commonStealthObjectView.VisionConeMesh == null)
                return;

            ConeMeshGenerator.UpdateMesh(_commonStealthObjectView.VisionConeMesh, range, angle, direction: direction);
        }

        public virtual void SetDetectionState(bool detected)
        {
            if (!detected)
                return;

            if (_commonStealthObjectView.InstanceMaterial != null)
            {
                Tween.MaterialColor(
                    _commonStealthObjectView.InstanceMaterial,
                    _commonStealthObjectView.DetectionColor,
                    0.2f,
                    Ease.OutQuad);
            }

            if (_commonStealthObjectView.DetectionSound != null)
                _commonStealthObjectView.DetectionSound.Play();

            if (_commonStealthObjectView.DetectionIndicatorView != null)
                _commonStealthObjectView.DetectionIndicatorView.Show();
        }

        protected virtual void OnDestroy()
        {
            if (_commonStealthObjectView.InstanceMaterial != null)
                Destroy(_commonStealthObjectView.InstanceMaterial);
        }
    }
}