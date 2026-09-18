using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdEvolutionSystem : ISystem, IService
    {
        public World World { get; set; }
        private readonly ConfigsService _configsService;
        private readonly StatsService _statsService;
        private Request<EvolutionCrowdRequest> _evolveRequest;

        public CrowdEvolutionSystem(ConfigsService configsService, StatsService statsService)
        {
            _configsService = configsService;
            _statsService = statsService;
        }

        public void OnAwake()
        {
            _evolveRequest = World.GetRequest<EvolutionCrowdRequest>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (EvolutionCrowdRequest request in _evolveRequest.Consume())
                Evolve(request.Member, request.EvolutionLevel);
        }

        public void Evolve(Entity member, int targetEvolutionLevel = 1)
        {
            if (member.IsNullOrDisposed() || targetEvolutionLevel <= 0)
                return;

            CrowdConfig config = _configsService.GetCrowdConfig();

            targetEvolutionLevel = Mathf.Min(targetEvolutionLevel, config.MaxEvolutionLevel);

            ref CrowdMemberEvolution evolution = ref member.GetComponent<CrowdMemberEvolution>();

            int evolutionsToAdd = targetEvolutionLevel - evolution.EvolutionLevel;

            for (int i = 0; i < evolutionsToAdd; i++)
            {
                ++evolution.EvolutionLevel;

                ApplyEvolutionStats(member, evolution.EvolutionLevel);

                bool isTierUp = config.IsTierUpEvolution(evolution.EvolutionLevel);

                if (isTierUp)
                    ApplyEvolutionSprite(member, evolution.EvolutionLevel);
            }
        }

        private void ApplyEvolutionStats(Entity member, int evolutionLevel)
        {
            CrowdConfig config = _configsService.GetCrowdConfig();
            CrowdMemberEvolutionConfig evolutionConfig = config.GetEvolution(evolutionLevel);

            ref Stats stats = ref member.GetComponent<Stats>();

            foreach ((StatId statId, _) in stats.Value)
            {
                float percent = evolutionConfig.GetStatPercent(statId);

                if (Mathf.Approximately(percent, 0f))
                    continue;

                _statsService.CreateStatModifier(
                    target: member,
                    modifierSetup: new SimpleStatModifierSetup
                    {
                        Operation = StatOperation.MULTIPLY,
                        ModifierValue = percent,
                        TargetStatId = statId,
                        SourceStatId = StatId.UNKNOWN,
                        AffectsBaseValue = true
                    },
                    producer: member
                );
            }
        }

        private void ApplyEvolutionSprite(Entity member, int evolutionLevel)
        {
            CrowdConfig config = _configsService.GetCrowdConfig();
            CrowdMemberEvolutionConfig evolutionConfig = config.GetEvolution(evolutionLevel);

            if (!evolutionConfig.HasSpriteChange)
                return;

            ref SpriteRendererLink spriteLink = ref member.GetComponent<SpriteRendererLink>();
            spriteLink.Value.sprite = evolutionConfig.Sprite;
        }

        public void Dispose()
        {
        }
    }
}