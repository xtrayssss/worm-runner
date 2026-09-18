using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LifeForceFeature;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EvolutionaryIncubatorVisualSystem : ISystem
    {
        public World World { get; set; }

        private Filter _incubatorsWithDisplay;
        private Event<EvolutionLevelUpEvent> _levelUpEvents;
        private readonly VFXService _vfxService;
        private readonly ConfigsService _configsService;

        public EvolutionaryIncubatorVisualSystem(VFXService vfxService, ConfigsService configsService)
        {
            _vfxService = vfxService;
            _configsService = configsService;
        }

        public void OnAwake()
        {
            _incubatorsWithDisplay = World.Filter
                .With<EvolutionaryIncubatorTag>()
                .With<EvolutionaryIncubatorState>()
                .With<EvolutionDisplaySettings>()
                .Without<EvolutionaryIncubatorDestroyMarker>()
                .Build();

            _levelUpEvents = World.GetEvent<EvolutionLevelUpEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            ProcessProgressUpdates();
            ProcessLevelUpEvents();
        }

        private void ProcessProgressUpdates()
        {
            foreach (Entity incubator in _incubatorsWithDisplay)
            {
                ref readonly EvolutionDisplaySettings displaySettings =
                    ref incubator.GetComponent<EvolutionDisplaySettings>();

                bool shouldUpdate = displaySettings.UpdateMode switch
                {
                    StatDisplayUpdateMode.ALWAYS => true,
                    _ => false
                };

                if (shouldUpdate)
                {
                    ref readonly EvolutionaryIncubatorState incubatorState =
                        ref incubator.GetComponent<EvolutionaryIncubatorState>();

                    int maxEvolutionLevel = incubatorState.DamageThresholdsPerLevel.Length;

                    if (incubatorState.TierLevel < maxEvolutionLevel)
                    {
                        float thresholdForCurrentLevel =
                            incubatorState.DamageThresholdsPerLevel[incubatorState.TierLevel];

                        displaySettings.Display.UpdateDisplay(
                            incubatorState.AccumulatedDamage,
                            thresholdForCurrentLevel);
                    }
                    else
                    {
                        displaySettings.Display.UpdateDisplay(1f, 1f);
                    }
                }
            }
        }

        private void ProcessLevelUpEvents()
        {
            foreach (EvolutionLevelUpEvent levelUpEvent in _levelUpEvents.publishedChanges)
            {
                Entity incubator = levelUpEvent.EvolutionaryIncubator;

                if (incubator.IsNullOrDisposed() || !incubator.Has<EvolutionDisplaySettings>())
                    continue;

                ref readonly EvolutionDisplaySettings displaySettings =
                    ref incubator.GetComponent<EvolutionDisplaySettings>();

                ref readonly EvolutionaryIncubatorState incubatorState =
                    ref incubator.GetComponent<EvolutionaryIncubatorState>();

                displaySettings.Display.UpdateLevel(incubatorState.TierLevel);

                ref readonly EntityViewLink viewLink = ref incubator.GetComponent<EntityViewLink>();
                EvolutionaryIncubatorView view = (EvolutionaryIncubatorView)viewLink.View;

                CrowdConfig crowdConfig = _configsService.GetCrowdConfig();

                int decorativeMemberEvolutionLevel = (incubatorState.TierLevel - 1) * crowdConfig.PipsPerTier + 1;

                view.UpdateMemberDisplay(
                    memberCount: incubatorState.SpawnCount,
                    evolutionLevel: decorativeMemberEvolutionLevel);

                displaySettings.Display.ResetProgress();

                _vfxService.PlayVFX(view.BuffVfx, view.transform.position, scale: 1.9f);
            }
        }

        public void Dispose()
        {
        }
    }
}