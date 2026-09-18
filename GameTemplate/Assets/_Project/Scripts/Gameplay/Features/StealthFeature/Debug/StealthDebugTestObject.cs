#if UNITY_EDITOR
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StealthFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Debug
{
    public sealed class StealthDebugTestObject : MonoBehaviour
    {
        [SerializeField]
        private Color _normalColor = Color.green;

        [SerializeField]
        private Color _detectedColor = Color.red;

        private bool _wasDetected;
        private Filter _detectors;

        private void Start() =>
            _detectors = World.Default!.Filter.With<VisionCone>().Build();

        private void Update() =>
            _wasDetected = CheckIfDetected();

        private bool CheckIfDetected()
        {
            Vector3 myPosition = transform.position;

            foreach (Entity detector in _detectors)
            {
                ref readonly VisionCone visionCone = ref detector.GetComponent<VisionCone>();
                ref readonly EntityViewLink viewLink = ref detector.GetComponent<EntityViewLink>();
                StealthObjectView stealthObjectView = (StealthObjectView)viewLink.View;
                CommonStealthObjectView commonStealthObjectView = stealthObjectView.CommonStealthObjectView;

                Vector3 detectorPosition = commonStealthObjectView.Cone.position;
                Vector3 forward = -commonStealthObjectView.ConePivot.forward;

                if (TargetingService.IsInVisionCone(
                        detectorPosition,
                        forward: forward,
                        myPosition,
                        visionCone.VisionRange,
                        visionCone.VisionAngle))
                {
                    return true;
                }
            }

            return false;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = _wasDetected ? _detectedColor : _normalColor;
            Gizmos.DrawWireSphere(transform.position, 6f);
        }
    }
}
#endif