using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.InputFeature.Systems;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdMovementCalculationSystem : ISystem
    {
        private readonly InputService _inputService;
        public World World { get; set; }

        private Filter _crowds;
        private Filter _crowdMembers;
        private readonly GameStateMachine _gameStateMachine;
        private readonly RunStatisticsService _runStatisticsService;

        public CrowdMovementCalculationSystem(
            InputService inputService,
            GameStateMachine gameStateMachine,
            RunStatisticsService runStatisticsService)
        {
            _inputService = inputService;
            _gameStateMachine = gameStateMachine;
            _runStatisticsService = runStatisticsService;
        }

        public void OnAwake()
        {
            _crowds = World.Filter
                .With<CrowdTag>()
                .With<MovementState>()
                .With<MovementSettings>()
                .Without<StopMovementMarker>()
                .Build();

            _crowdMembers = World.Filter
                .With<CrowdMemberTag>()
                .Without<CrowdMemberDyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_crowdMembers.IsEmpty())
                return;

            if (_gameStateMachine.CurrentState is not GameplayState ||
                _runStatisticsService.CurrentRun.IsBonusChestTriggered)
                return;

            foreach (Entity crowd in _crowds)
            {
                ref readonly EntityViewLink entityViewLink = ref crowd.GetComponent<EntityViewLink>();
                ref readonly MovementSettings settings = ref crowd.GetComponent<MovementSettings>();
                ref MovementState state = ref crowd.GetComponent<MovementState>();

                Vector3 horizontalMovementRaw = entityViewLink.View.transform.right * _inputService.MoveDirection;
                Vector3 forwardMovementRaw = entityViewLink.View.transform.forward * settings.ForwardMoveSpeed;

                state.HorizontalMovement = horizontalMovementRaw * settings.HorizontalMoveSpeed;

                state.ForwardMovement = Vector3.SmoothDamp(
                    state.ForwardMovement,
                    forwardMovementRaw,
                    ref state.ForwardVelocity,
                    settings.ForwardSmoothingTime);
            }
        }

        public void Dispose()
        {
        }
    }
}