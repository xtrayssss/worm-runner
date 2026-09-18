using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FinishTriggerSystem : ISystem
    {
        public World World { get; set; }
        private Event<TriggerEnterEvent> _triggerEnterEvent;
        private Event<FinishReachedEvent> _finishReachedEvent;

        private readonly LevelService _levelService;
        private readonly CameraService _cameraService;
        private readonly CollectibleService _collectibleService;
        private readonly CaptureZoneService _captureZoneService;

        public FinishTriggerSystem(
            LevelService levelService,
            CameraService cameraService,
            CollectibleService collectibleService,
            CaptureZoneService captureZoneService)
        {
            _levelService = levelService;
            _cameraService = cameraService;
            _collectibleService = collectibleService;
            _captureZoneService = captureZoneService;
        }

        public void OnAwake()
        {
            _triggerEnterEvent = World.GetEvent<TriggerEnterEvent>();
            _finishReachedEvent = World.GetEvent<FinishReachedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (TriggerEnterEvent triggerEnterEvent in _triggerEnterEvent.publishedChanges)
            {
                Entity trigger = triggerEnterEvent.Source.Entity;

                if (trigger.IsNullOrDisposed() || !trigger.Has<FinishTrigger>())
                    continue;

                ref FinishTrigger finishTrigger = ref trigger.GetComponent<FinishTrigger>();

                if (finishTrigger.IsTriggered)
                    continue;

                finishTrigger.IsTriggered = true;

                ref readonly EntityViewLink triggerViewLink = ref trigger.GetComponent<EntityViewLink>();

                FinishTriggerView finishView = (FinishTriggerView)triggerViewLink.View;
                finishView.PlayConfettiEffects();

                switch (_levelService.CurrentLevel.LevelMode)
                {
                    case LevelMode.COLLECTOR:
                        HandleCollectorModeFinish();
                        break;
                    case LevelMode.CAPTURE_ZONES:
                        HandleCaptureZoneModeFinish();
                        break;
                    default:
                        _levelService.SetLevelCompleted(_levelService.CurrentLevelIndex);
                        break;
                }

                _finishReachedEvent.NextFrame(new FinishReachedEvent());

                _cameraService.PlayFinishFovEffect(fovIncrease: -10f, duration: 0.7f);
            }
        }

        private void HandleCaptureZoneModeFinish() =>
            _levelService.SetLevelCompleted(_levelService.CurrentLevelIndex, _captureZoneService.HasRequiredCompletion);

        private void HandleCollectorModeFinish() =>
            _levelService.SetLevelCompleted(_levelService.CurrentLevelIndex, _collectibleService.AllCollected);

        public void Dispose()
        {
        }
    }
}