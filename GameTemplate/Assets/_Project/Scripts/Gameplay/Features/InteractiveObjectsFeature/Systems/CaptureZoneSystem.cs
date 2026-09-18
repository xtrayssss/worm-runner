using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CaptureZoneSystem : ISystem
    {
        public World World { get; set; }

        private Filter _crowds;
        private Filter _captureZones;
        private Request<RemoveCrowdMembersRequest> _removeMembersRequest;

        private readonly CrowdFactory _crowdFactory;
        private readonly CaptureZoneService _captureZoneService;
        private readonly UIRoot _uiRoot;
        private readonly AudioService _audioService;
        private readonly ConfigsService _configsService;

        public CaptureZoneSystem(
            CrowdFactory crowdFactory,
            CaptureZoneService captureZoneService,
            UIRoot uiRoot,
            AudioService audioService,
            ConfigsService configsService)
        {
            _crowdFactory = crowdFactory;
            _captureZoneService = captureZoneService;
            _uiRoot = uiRoot;
            _audioService = audioService;
            _configsService = configsService;
        }

        public void OnAwake()
        {
            _captureZones = World.Filter
                .With<CaptureZoneTag>()
                .With<CaptureZoneState>()
                .With<ActiveTrigger>()
                .Without<CaptureZoneDestroyMarker>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .With<CrowdPopulation>()
                .Build();

            _removeMembersRequest = World.GetRequest<RemoveCrowdMembersRequest>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_crowds.IsEmpty())
                return;

            Entity crowd = _crowds.First();
            ref readonly CrowdPopulation population = ref crowd.GetComponent<CrowdPopulation>();
            int currentMilitaryCount = population.MilitaryPopulation;

            foreach (Entity zone in _captureZones)
            {
                ref ActiveTrigger trigger = ref zone.GetComponent<ActiveTrigger>();
                trigger.IsProcessed = true;

                ref CaptureZoneState zoneState = ref zone.GetComponent<CaptureZoneState>();

                List<Entity> membersInZone = CountMembersInZone(trigger);

                int alreadyCaptured = zoneState.CapturedMembers.Count;
                int remainingToCapture = zoneState.RequiredMembers - alreadyCaptured;
                int availableToCapture = membersInZone.Count;

                int maxCanCapture = Mathf.Max(0, currentMilitaryCount - 1);
                int actualCapture = Mathf.Min(remainingToCapture, Mathf.Min(availableToCapture, maxCanCapture));

                if (actualCapture <= 0)
                    continue;

                List<Entity> membersToCapture = membersInZone.Take(actualCapture).ToList();
                zoneState.CapturedMembers.AddRange(membersToCapture);
                currentMilitaryCount -= actualCapture;

                ref EntityViewLink zoneViewLink = ref zone.GetComponent<EntityViewLink>();
                CaptureZoneView zoneView = (CaptureZoneView)zoneViewLink.View;

                zoneView.UpdateStatusLabel(zoneState.CapturedMembers.Count, zoneState.RequiredMembers);

                CaptureMembers(zoneView, in zoneState);

                _audioService.PlaySound(AudioId.Sfx.Gameplay.ZONE_CAPTURE);

                if (zoneState.CapturedMembers.Count >= zoneState.RequiredMembers)
                    CaptureZone(zone);
            }
        }

        private List<Entity> CountMembersInZone(in ActiveTrigger trigger) =>
            trigger.Triggers
                .Where(static info => !info.Other.Entity.IsNullOrDisposed() &&
                                      info.Other.Entity.Has<MilitaryCrowdMemberTag>() &&
                                      !info.Other.Entity.Has<CapturedMemberMarker>())
                .Select(static info => info.Other.Entity)
                .ToList();

        private void CaptureZone(Entity zone)
        {
            ref ColliderLink colliderLink = ref zone.GetComponent<ColliderLink>();
            colliderLink.Value.enabled = false;

            zone.AddComponent<CaptureZoneDestroyMarker>();
        }

        private void CaptureMembers(CaptureZoneView zoneView, in CaptureZoneState zoneState)
        {
            if (zoneState.CapturedMembers.Count > 0)
            {
                _removeMembersRequest.Publish(new RemoveCrowdMembersRequest
                {
                    SpecificMembers = zoneState.CapturedMembers
                        .Where(member => !member.IsNullOrDisposed() && !member.Has<CapturedMemberMarker>()).ToArray(),
                    SuppressSoulEffect = true
                }, allowNextFrame: true);
            }

            for (int index = 0; index < zoneState.CapturedMembers.Count; index++)
            {
                Entity capturedMember = zoneState.CapturedMembers[index];

                if (capturedMember.IsNullOrDisposed() || capturedMember.Has<CapturedMemberMarker>())
                    continue;

                ref readonly CrowdMemberEvolution memberEvolution =
                    ref capturedMember.GetComponent<CrowdMemberEvolution>();

                CrowdConfig crowdConfig = _configsService.GetCrowdConfig();

                int decorativeMemberEvolutionLevel = crowdConfig.GetTierFromEvolution(memberEvolution.EvolutionLevel) *
                    crowdConfig.PipsPerTier + 1;

                DecorativeMemberView decorativeMemberView = _crowdFactory.CreateDecorativeMember(
                    zoneView.MemberSpawnPoints[index].position,
                    zoneView.MembersContainer,
                    evolutionLevel: decorativeMemberEvolutionLevel);

                decorativeMemberView.transform.rotation = Quaternion.Euler(25, 0, 0);

                decorativeMemberView.StartIdle();

                _captureZoneService.UpdateCompletion();

                decorativeMemberView.PlayPunchAnimation();

                capturedMember.AddComponent<CapturedMemberMarker>();

                _uiRoot.GameWindow.ObjectiveLabel.UpdateObjectiveDisplay();
            }
        }

        public void Dispose()
        {
        }
    }
}