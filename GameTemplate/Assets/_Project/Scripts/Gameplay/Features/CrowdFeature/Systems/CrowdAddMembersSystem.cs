using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    public sealed class CrowdAddMembersSystem : ISystem
    {
        public World World { get; set; }

        private readonly CrowdFactory _crowdFactory;
        private readonly CrowdTriangleFormationService _crowdTriangleFormationService;

        private Request<AddCrowdMembersRequest> _addMembersRequest;
        private Event<MilitaryPopulationChangedEvent> _populationChangedEvent;
        private Filter _crowds;
        private Filter _crowdMembers;

        private readonly struct MemberTypeData
        {
            public readonly List<Entity> Members;
            public readonly int TotalPopulation;
            public readonly Vector3 PositionOffset;

            public MemberTypeData(List<Entity> members, int totalPopulation, Vector3 positionOffset)
            {
                Members = members;
                TotalPopulation = totalPopulation;
                PositionOffset = positionOffset;
            }
        }

        public CrowdAddMembersSystem(
            CrowdFactory crowdFactory,
            CrowdTriangleFormationService crowdTriangleFormationService)
        {
            _crowdFactory = crowdFactory;
            _crowdTriangleFormationService = crowdTriangleFormationService;
        }

        public void OnAwake()
        {
            _addMembersRequest = World.GetRequest<AddCrowdMembersRequest>();
            _populationChangedEvent = World.GetEvent<MilitaryPopulationChangedEvent>();

            _crowds = World.Filter
                .With<CrowdPopulation>()
                .With<CrowdTag>()
                .Build();

            _crowdMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<EntityViewLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (AddCrowdMembersRequest request in _addMembersRequest.Consume())
            {
                if (request.Count <= 0)
                    continue;

                Entity crowd = _crowds.First();
                ref CrowdPopulation population = ref crowd.GetComponent<CrowdPopulation>();

                int actualAddCount = request.MemberType == CrowdMemberType.MILITARY
                    ? Mathf.Min(request.Count, population.MaxMilitaryPopulation - population.MilitaryPopulation)
                    : request.Count;

                if (actualAddCount <= 0)
                    continue;

                if (request.MemberType == CrowdMemberType.MILITARY)
                    population.MilitaryPopulation += actualAddCount;
                else
                    population.CivilianPopulation += actualAddCount;

                MemberTypeData typeData = request.MemberType == CrowdMemberType.MILITARY
                    ? new MemberTypeData(population.MilitaryMembers, population.MilitaryPopulation, Vector3.zero)
                    : new MemberTypeData(population.CivilianMembers, population.CivilianPopulation,
                        new Vector3(0f, 0f, -3f));

                for (int i = 0; i < actualAddCount; i++)
                {
                    int formationIndex = typeData.Members.Count;

                    Entity member = _crowdFactory.CreateCrowdMember(
                        request.EvolutionLevel,
                        formationIndex,
                        request.MemberType);

                    typeData.Members.Add(member);

                    ref EntityViewLink viewLink = ref member.GetComponent<EntityViewLink>();

                    member.AddComponent<CrowdMemberFormation>() = new CrowdMemberFormation
                    {
                        FormationIndex = formationIndex
                    };

                    Vector3 position = _crowdTriangleFormationService.GetTrianglePosition(
                        formationIndex,
                        typeData.TotalPopulation) + typeData.PositionOffset;

                    viewLink.View.transform.localPosition = position + Vector3.up * 1.2f;

                    SetAllMembersAnimation(request.AnimationType);
                    SetMemberAnimation(request.AnimationType, (CrowdMemberView)viewLink.View);
                }

                _populationChangedEvent.NextFrame(new MilitaryPopulationChangedEvent
                {
                    NewPopulation = population.MilitaryPopulation,
                    PopulationDelta = actualAddCount
                });
            }
        }

        private void SetAllMembersAnimation(CrowdMemberAnimationType animationType)
        {
            foreach (Entity member in _crowdMembers)
            {
                ref readonly EntityViewLink memberViewLink = ref member.GetComponent<EntityViewLink>();
                CrowdMemberView view = (CrowdMemberView)memberViewLink.View;
                SetMemberAnimation(animationType, view);
            }
        }

        private void SetMemberAnimation(CrowdMemberAnimationType animationType, CrowdMemberView view)
        {
            view.StopAllAnimations();

            switch (animationType)
            {
                case CrowdMemberAnimationType.IDLE:
                    view.StartIdle();
                    break;
                case CrowdMemberAnimationType.JUMP:
                    view.StartJump();
                    break;
                case CrowdMemberAnimationType.NONE:
                default:
                    break;
            }
        }

        public void Dispose()
        {
        }
    }
}