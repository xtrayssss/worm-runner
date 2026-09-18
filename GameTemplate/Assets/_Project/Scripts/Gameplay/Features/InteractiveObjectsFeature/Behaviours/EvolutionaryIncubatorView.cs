using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours
{
    public sealed class EvolutionaryIncubatorView : InteractiveObjectView
    {
        [Header("Evolutionary Incubator Specific")]
        [SerializeField] private EvolutionProgressDisplay _evolutionProgressDisplay;

        [Header("Member Display Configurations")]
        [SerializeField] private MemberDisplayConfiguration[] _memberConfigurations = new MemberDisplayConfiguration[5];

        private ConfigsService _configsService;

        private CrowdFactory _crowdFactory;
        private readonly List<DecorativeMemberView> _currentMembers = new List<DecorativeMemberView>(capacity: 8);
        public EvolutionProgressDisplay EvolutionProgressDisplay => _evolutionProgressDisplay;

        public void Construct(ConfigsService configsService, CrowdFactory crowdFactory)
        {
            _configsService = configsService;
            _crowdFactory = crowdFactory;
            _evolutionProgressDisplay.Construct();
        }

        public void UpdateMemberDisplay(int memberCount, int evolutionLevel)
        {
            foreach (DecorativeMemberView member in _currentMembers)
                DestroyImmediate(member.gameObject);

            _currentMembers.Clear();

            MemberDisplayConfiguration config = _memberConfigurations[memberCount - 1];

            foreach (Transform point in config.MemberPositions)
            {
                DecorativeMemberView memberView = CreateMember(point, evolutionLevel, parent: point);

                memberView.StartIdle();

                _currentMembers.Add(memberView);
            }
        }

        public void PlayActivationVFX(Vector3 position)
        {
            VFXData upgradeVFX = _configsService.GetGameConfig().UpgradeVFX;

            VFXService.PlayVFX(upgradeVFX, position);
        }

        private DecorativeMemberView CreateMember(Transform spawnPoint, int evolutionLevel, Transform parent)
        {
            DecorativeMemberView decorativeMemberView = _crowdFactory.CreateDecorativeMember(
                spawnPoint.position,
                parent: parent,
                evolutionLevel);

            decorativeMemberView.transform.localRotation = Quaternion.identity;

            return decorativeMemberView;
        }

        [Serializable]
        public class MemberDisplayConfiguration
        {
            public Transform[] MemberPositions;
        }
    }
}