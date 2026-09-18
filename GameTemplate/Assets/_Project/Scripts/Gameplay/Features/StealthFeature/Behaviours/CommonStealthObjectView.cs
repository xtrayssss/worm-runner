using JetBrains.Annotations;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours
{
    public class CommonStealthObjectView : MonoBehaviour
    {
        [SerializeField]
        private MeshFilter _visionConeMesh;

        [SerializeField]
        private MeshRenderer _visionConeRenderer;

        [SerializeField]
        private Material _visionConeMaterial;

        [SerializeField]
        private Color _normalColor = new Color(1f, 1, 1, 1f);

        [SerializeField]
        private Color _detectionColor = new Color(1f, 0f, 0f, 1f);

        [SerializeField]
        private AudioSource _detectionSound;

        [SerializeField] private Transform _conePivot;
        
        [SerializeField]
        [CanBeNull]
        private DetectionIndicatorView _detectionIndicatorView;

        [CanBeNull]
        public DetectionIndicatorView DetectionIndicatorView => _detectionIndicatorView;

        public Transform ConePivot => _conePivot;
        public Material InstanceMaterial { get; set; }

        public AudioSource DetectionSound => _detectionSound;
        public Color NormalColor => _normalColor;
        public Color DetectionColor => _detectionColor;
        public MeshFilter VisionConeMesh => _visionConeMesh;
        public MeshRenderer VisionConeRenderer => _visionConeRenderer;
        public Material VisionConeMaterial => _visionConeMaterial;
        public Transform Cone => _visionConeMesh.transform;
    }
}