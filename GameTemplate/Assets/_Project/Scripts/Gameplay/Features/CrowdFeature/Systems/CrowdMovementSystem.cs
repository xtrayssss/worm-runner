using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdMovementSystem : ISystem
    {
        public World World { get; set; }

        private Filter _crowds;
        private Filter _runningCrowdMembers;
        private readonly GameStateMachine _gameStateMachine;
        private readonly RunStatisticsService _runStatisticsService;
        private readonly UIRoot _uiRoot;

        public CrowdMovementSystem(
            GameStateMachine gameStateMachine,
            RunStatisticsService runStatisticsService,
            UIRoot uiRoot)
        {
            _gameStateMachine = gameStateMachine;
            _runStatisticsService = runStatisticsService;
            _uiRoot = uiRoot;
        }

        public void OnAwake()
        {
            _crowds = World.Filter
                .With<CrowdTag>()
                .With<MovementState>()
                .With<CharacterControllerLink>()
                .With<CrowdMovableBounds>()
                .Without<StopMovementMarker>()
                .Build();

            _runningCrowdMembers = World.Filter
                .With<CrowdMemberTag>()
                .Without<CrowdMemberDyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_runningCrowdMembers.IsEmpty())
                return;

            if (_gameStateMachine.CurrentState is not GameplayState ||
                _runStatisticsService.CurrentRun.IsBonusChestTriggered)
                return;

            foreach (Entity entity in _crowds)
            {
                ref EntityViewLink crowdViewLink = ref entity.GetComponent<EntityViewLink>();
                ref MovementState movementState = ref entity.GetComponent<MovementState>();
                ref CharacterControllerLink controllerLink = ref entity.GetComponent<CharacterControllerLink>();

                Vector3 finalMovement = (movementState.HorizontalMovement + movementState.ForwardMovement) * deltaTime;

                finalMovement = GetConstrainedMotion(finalMovement, crowdViewLink.View);

                controllerLink.Controller.Move(finalMovement);
                
                float forwardDistance = finalMovement.z;

                if (forwardDistance > 0)
                {
                    _runStatisticsService.SetDistanceTraveled(crowdViewLink.View.transform.position.z);
                    _uiRoot.GameWindow.DistanceProgressBar.UpdateProgress(crowdViewLink.View.transform.position.z);
                }
            }
        }

        public void Dispose()
        {
        }

        private static Vector3 GetConstrainedMotion(Vector3 motionVector, EntityView crowdView)
        {
            Vector3 position = crowdView.transform.position;
            ref CrowdMovableBounds movableBounds = ref crowdView.Entity.GetComponent<CrowdMovableBounds>();

            if (position.x > 4.5f - movableBounds.Value.extents.x)
            {
                if (motionVector.x > 0)
                {
                    return new Vector3(0f, motionVector.y, motionVector.z);
                }
            }
            else if (position.x < -4.5f + movableBounds.Value.extents.x)
            {
                if (motionVector.x < 0)
                {
                    return new Vector3(0f, motionVector.y, motionVector.z);
                }
            }

            return motionVector;
        }
    }
}