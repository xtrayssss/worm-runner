using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using PrimeTween;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class OrganizeCrowdSystem : ISystem
    {
        public World World { get; set; }

        private Event<MilitaryPopulationChangedEvent> _populationChangedEvent;
        private Filter _crowds;
        private Filter _crowdMembers;
        private readonly CrowdTriangleFormationService _crowdTriangleFormationService;
        private readonly GameStateMachine _gameStateMachine;
        private readonly RunStatisticsService _runStatisticsService;

        public OrganizeCrowdSystem(
            CrowdTriangleFormationService crowdTriangleFormationService,
            GameStateMachine gameStateMachine,
            RunStatisticsService runStatisticsService)
        {
            _crowdTriangleFormationService = crowdTriangleFormationService;
            _gameStateMachine = gameStateMachine;
            _runStatisticsService = runStatisticsService;
        }

        private const float ORGANIZE_DELAY = 1f;

        public void OnAwake()
        {
            _populationChangedEvent = World.GetEvent<MilitaryPopulationChangedEvent>();

            _crowds = World.Filter
                .With<CrowdTag>()
                .With<CrowdOrganization>()
                .Build();

            _crowdMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<EntityViewLink>()
                .With<CrowdMemberFormation>()
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

            foreach (MilitaryPopulationChangedEvent _ in _populationChangedEvent.publishedChanges)
            {
                Entity crowd = _crowds.First();

                ref CrowdOrganization crowdOrganization = ref crowd.GetComponent<CrowdOrganization>();

                if (crowdOrganization.OrganizationSequence.isAlive)
                    crowdOrganization.OrganizationSequence.Stop();

                Sequence sequence = Sequence.Create();
                Sequence movementSequence = Sequence.Create();

                sequence
                    .Chain(Tween.Delay(duration: ORGANIZE_DELAY));

                foreach (Entity member in _crowdMembers)
                {
                    ref readonly EntityViewLink memberViewLink = ref member.GetComponent<EntityViewLink>();
                    ref readonly CrowdMemberFormation memberFormation = ref member.GetComponent<CrowdMemberFormation>();
                    ref readonly CrowdPopulation crowdPopulation = ref crowd.GetComponent<CrowdPopulation>();

                    Vector3 targetLocalPosition = _crowdTriangleFormationService.GetTrianglePosition(
                        memberFormation.FormationIndex,
                        crowdPopulation.MilitaryPopulation);

                    movementSequence
                        .Group(Tween.LocalPositionX(
                            target: memberViewLink.View.transform,
                            endValue: targetLocalPosition.x,
                            duration: crowdOrganization.OrganizationDuration,
                            ease: Ease.OutBack))
                        .Group(Tween.LocalPositionZ(
                            target: memberViewLink.View.transform,
                            endValue: targetLocalPosition.z,
                            duration: crowdOrganization.OrganizationDuration,
                            ease: Ease.OutBack));
                }

                crowdOrganization.OrganizationSequence = sequence;

                sequence.Chain(movementSequence);
            }
        }

        public void Dispose()
        {
        }
    }
}