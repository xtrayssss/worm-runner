using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StealthFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class VisionDetectionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _detectors;
        private Filter _crowdMembers;
        private Event<MemberDetectionEvent> _detectionEvent;

        private readonly AudioService _audioService;

        public VisionDetectionSystem(AudioService audioService) =>
            _audioService = audioService;

        public void OnAwake()
        {
            _detectors = World.Filter
                .With<VisionCone>()
                .With<EntityViewLink>()
                .Without<DetectedTargetMarker>()
                .Build();

            _crowdMembers = World.Filter
                .With<CrowdMemberTag>()
                .With<EntityViewLink>()
                .Without<CrowdMemberDyingMarker>()
                .Build();

            _detectionEvent = World.GetEvent<MemberDetectionEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity detector in _detectors)
            {
                ref readonly VisionCone visionCone = ref detector.GetComponent<VisionCone>();
                ref readonly EntityViewLink detectorViewLink = ref detector.GetComponent<EntityViewLink>();

                StealthObjectView detectorView = (StealthObjectView)detectorViewLink.View;

                bool detectedAny = false;

                foreach (Entity member in _crowdMembers)
                {
                    ref readonly EntityViewLink memberViewLink = ref member.GetComponent<EntityViewLink>();
                    CommonStealthObjectView detectorCommonView = detectorView.CommonStealthObjectView;

                    if (TargetingService.IsInVisionCone(
                            origin: detectorCommonView.Cone.position,
                            forward: -detectorCommonView.ConePivot.forward,
                            target: memberViewLink.View.transform.position,
                            range: visionCone.VisionRange,
                            angle: visionCone.VisionAngle))
                    {
                        detectedAny = true;

                        _detectionEvent.NextFrame(new MemberDetectionEvent
                        {
                            DetectedEntity = member,
                            Detector = detector,
                            DetectionPoint = memberViewLink.View.transform.position
                        });

                        break;
                    }
                }

                if (detectedAny)
                {
                    detector.AddComponent<DetectedTargetMarker>();
                    _audioService.PlaySound(AudioId.Sfx.Gameplay.DETECTED);
                }

                detectorView.SetDetectionState(detectedAny);
            }
        }

        public void Dispose()
        {
        }
    }
}