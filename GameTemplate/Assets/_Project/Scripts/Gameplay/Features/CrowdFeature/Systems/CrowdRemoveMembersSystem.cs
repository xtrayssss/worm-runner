using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Extensions;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdRemoveMembersSystem : ISystem, IService
    {
        public World World { get; set; }

        private Request<RemoveCrowdMembersRequest> _removeMembersRequest;
        private Event<MilitaryPopulationChangedEvent> _militaryPopulationChangedEvent;
        private Event<CrowdExtinctEvent> _crowdExtinctEvent;
        private Event<CrowdMemberRemovedEvent> _memberRemovedEvent;

        private Filter _militaryMembers;
        private Filter _civilianMembers;
        private Filter _crowds;

        private readonly RunStatisticsService _runStatisticsService;
        private readonly SoulFactory _soulFactory;

        private readonly struct MemberTypeData
        {
            public readonly List<Entity> Members;
            public readonly int CurrentPopulation;
            public readonly Filter MembersFilter;

            public MemberTypeData(
                List<Entity> members,
                int currentPopulation,
                Filter membersFilter)
            {
                Members = members;
                CurrentPopulation = currentPopulation;
                MembersFilter = membersFilter;
            }
        }

        public CrowdRemoveMembersSystem(
            RunStatisticsService runStatisticsService,
            SoulFactory soulFactory)
        {
            _runStatisticsService = runStatisticsService;
            _soulFactory = soulFactory;
        }

        public void OnAwake()
        {
            _removeMembersRequest = World.GetRequest<RemoveCrowdMembersRequest>();
            _militaryPopulationChangedEvent = World.GetEvent<MilitaryPopulationChangedEvent>();
            _crowdExtinctEvent = World.GetEvent<CrowdExtinctEvent>();
            _memberRemovedEvent = World.GetEvent<CrowdMemberRemovedEvent>();

            _militaryMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .Build();

            _civilianMembers = World.Filter
                .With<CivilianCrowdMemberTag>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .With<CrowdPopulation>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (RemoveCrowdMembersRequest request in _removeMembersRequest.Consume())
            {
                if (_crowds.IsEmpty())
                    continue;

                ProcessRemovalRequest(request);
            }
        }

        public async UniTask RemoveAllMilitaryMembersWithDissolveAsync()
        {
            if (_crowds.IsEmpty())
                return;

            Entity crowd = _crowds.First();

            CrowdPopulation population = crowd.GetComponent<CrowdPopulation>();

            if (population.MilitaryPopulation == 0)
                return;

            MemberTypeData typeData = new MemberTypeData(
                population.MilitaryMembers,
                population.MilitaryPopulation,
                _militaryMembers);

            List<Entity> allMembers = typeData.Members.ToList();
            List<UniTask> dissolveTasks = new List<UniTask>();

            foreach (Entity member in allMembers)
            {
                if (member.IsNullOrDisposed())
                    continue;

                RigidbodyLink rigidbodyLink = member.GetComponent<RigidbodyLink>();
                rigidbodyLink.Value.velocity = Vector3.zero;
                rigidbodyLink.Value.isKinematic = true;

                ColliderLink colliderLink = member.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                EntityViewLink viewLink = member.GetComponent<EntityViewLink>();
                CrowdMemberView memberView = (CrowdMemberView)viewLink.View;

                memberView.StopJump();

                dissolveTasks.Add(memberView.PlayDissolveEffectAsync());
            }

            await UniTask.WhenAll(dissolveTasks);

            int removeCount = allMembers.Count;

            foreach (Entity member in allMembers)
            {
                if (member.IsNullOrDisposed())
                    continue;

                EntityViewLink viewLink = member.GetComponent<EntityViewLink>();
                Object.Destroy(viewLink.View.gameObject);
                World.RemoveEntity(member);
            }

            crowd.GetComponent<CrowdPopulation>().MilitaryPopulation = 0;
            crowd.GetComponent<CrowdPopulation>().MilitaryMembers.Clear();

            _militaryPopulationChangedEvent.NextFrame(new MilitaryPopulationChangedEvent
            {
                NewPopulation = 0,
                PopulationDelta = -removeCount
            });

            _crowdExtinctEvent.NextFrame(new CrowdExtinctEvent());
        }

        private void ProcessRemovalRequest(RemoveCrowdMembersRequest request)
        {
            Entity crowd = _crowds.First();
            ref CrowdPopulation population = ref crowd.GetComponent<CrowdPopulation>();

            MemberTypeData typeData = request.MemberType == CrowdMemberType.MILITARY
                ? new MemberTypeData(
                    population.MilitaryMembers,
                    population.MilitaryPopulation,
                    _militaryMembers)
                : new MemberTypeData(
                    population.CivilianMembers,
                    population.CivilianPopulation,
                    _civilianMembers);

            int actualRemoveCount = 0;

            if (!request.SpecificMember.IsNullOrDisposed())
            {
                DestroyMember(request.SpecificMember, in typeData, request.SuppressSoulEffect);
                actualRemoveCount++;
            }
            else if (request.SpecificMembers != null && request.SpecificMembers.Length > 0)
            {
                foreach (Entity member in request.SpecificMembers)
                {
                    DestroyMember(member, in typeData, request.SuppressSoulEffect);
                    actualRemoveCount++;
                }
            }
            else
            {
                actualRemoveCount = Mathf.Min(request.Count, typeData.CurrentPopulation);

                if (actualRemoveCount <= 0)
                    return;

                DestroyRandomMembers(actualRemoveCount, typeData, request.SuppressSoulEffect);
            }

            if (request.MemberType == CrowdMemberType.MILITARY)
                population.MilitaryPopulation -= actualRemoveCount;
            else
                population.CivilianPopulation -= actualRemoveCount;

            UpdateFormationIndices(typeData.Members);

            PublishEvents(
                request.MemberType,
                typeData.CurrentPopulation - actualRemoveCount,
                actualRemoveCount);

            if (request.MemberType == CrowdMemberType.MILITARY &&
                population.MilitaryPopulation == 0)
            {
                _crowdExtinctEvent.NextFrame(new CrowdExtinctEvent());
            }
        }

        private void DestroyRandomMembers(int count, MemberTypeData typeData, bool suppressSoulEffect)
        {
            int destroyedCount = 0;

            foreach (Entity member in typeData.MembersFilter.ToList().OrderBy(e => e.Id))
            {
                if (destroyedCount >= count)
                    break;

                DestroyMember(member, in typeData, suppressSoulEffect);
                destroyedCount++;
            }
        }

        private void DestroyMember(Entity member, in MemberTypeData typeData, bool suppressSoulEffect)
        {
            if (member.IsNullOrDisposed())
                return;

            ref readonly EntityViewLink viewLink = ref member.GetComponent<EntityViewLink>();

            if (!suppressSoulEffect)
            {
                _soulFactory
                    .CreateSoul(viewLink.View.transform.position, SoulType.FRIENDLY)
                    .PlaySoulAnimation();
            }

            Object.Destroy(viewLink.View.gameObject);
            World.RemoveEntity(member);

            _runStatisticsService.AddCrowdMemberLost();

            typeData.Members.Remove(member);
        }

        private void UpdateFormationIndices(List<Entity> members)
        {
            for (int i = 0; i < members.Count; i++)
            {
                Entity member = members[i];
                ref CrowdMemberFormation formation = ref member.GetComponent<CrowdMemberFormation>();
                formation.FormationIndex = i;
            }
        }

        private void PublishEvents(CrowdMemberType memberType, int newPopulation, int removedCount)
        {
            if (memberType == CrowdMemberType.MILITARY)
            {
                _militaryPopulationChangedEvent.NextFrame(new MilitaryPopulationChangedEvent
                {
                    NewPopulation = newPopulation,
                    PopulationDelta = -removedCount
                });
            }

            for (int i = 0; i < removedCount; i++)
            {
                _memberRemovedEvent.NextFrame(new CrowdMemberRemovedEvent
                {
                    RemovedMember = default
                });
            }
        }

        public void Dispose()
        {
        }
    }
}