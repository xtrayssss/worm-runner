using System.Linq;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EvolutionaryIncubatorActivationSystem : ISystem
    {
        public World World { get; set; }

        private Filter _incubators;
        private Filter _crowds;
        private Request<AddCrowdMembersRequest> _addMembersRequest;

        private readonly ConfigsService _configsService;

        public EvolutionaryIncubatorActivationSystem(ConfigsService configsService)
        {
            _configsService = configsService;
        }

        public void OnAwake()
        {
            _incubators = World.Filter
                .With<EvolutionaryIncubatorTag>()
                .With<EvolutionaryIncubatorState>()
                .With<ActiveTrigger>()
                .Without<EvolutionaryIncubatorDestroyMarker>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .Build();

            _addMembersRequest = World.GetRequest<AddCrowdMembersRequest>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_crowds.IsEmpty())
                return;

            foreach (Entity incubator in _incubators)
            {
                ref readonly ActiveTrigger trigger = ref incubator.GetComponent<ActiveTrigger>();

                bool shouldActivate = CheckActivationConditions(trigger);

                if (shouldActivate)
                    ActivateEvolutionaryIncubator(incubator);
            }
        }

        private void ActivateEvolutionaryIncubator(Entity incubator)
        {
            ref readonly EvolutionaryIncubatorState incubatorState =
                ref incubator.GetComponent<EvolutionaryIncubatorState>();

            CrowdConfig crowdConfig = _configsService.GetCrowdConfig();

            _addMembersRequest.Publish(new AddCrowdMembersRequest
            {
                Count = incubatorState.SpawnCount,
                EvolutionLevel = incubatorState.TierLevel * crowdConfig.PipsPerTier,
                AnimationType = CrowdMemberAnimationType.JUMP
            }, allowNextFrame: true);

            ref EntityViewLink incubatorViewLink = ref incubator.GetComponent<EntityViewLink>();
            EvolutionaryIncubatorView incubatoView = (EvolutionaryIncubatorView)incubatorViewLink.View;

            Entity crowd = _crowds.First();

            ref readonly EntityViewLink crowdViewLink = ref crowd.GetComponent<EntityViewLink>();
            incubatoView.PlayActivationVFX(crowdViewLink.View.transform.position);

            incubator.AddComponent<EvolutionaryIncubatorDestroyMarker>();
        }

        private static bool CheckActivationConditions(in ActiveTrigger trigger)
        {
            return trigger.Triggers
                .Any(static triggerInfo => !triggerInfo.Other.Entity.IsNullOrDisposed() &&
                                           triggerInfo.Other.Entity.Has<MilitaryCrowdMemberTag>());
        }

        public void Dispose()
        {
        }
    }
}