using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnemyCountTrackingSystem : ISystem
    {
        public World World { get; set; }

        private Filter _spawners;
        private readonly LevelService _levelService;
        private readonly GameStateMachine _gameStateMachine;
        private Event<AllEnemiesDefeatedEvent> _allEnemiesDefeatedEvent;

        public EnemyCountTrackingSystem(
            LevelService levelService,
            GameStateMachine gameStateMachine)
        {
            _levelService = levelService;
            _gameStateMachine = gameStateMachine;
        }

        public void OnAwake()
        {
            _allEnemiesDefeatedEvent = World.GetEvent<AllEnemiesDefeatedEvent>();

            _spawners = World.Filter
                .With<SpawnerState>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState ||
                _levelService.CurrentLevel.LevelMode != LevelMode.DEFENSE)
                return;

            if (_spawners.IsEmpty())
                return;

            Entity spawner = _spawners.First();

            ref SpawnerState state = ref spawner.GetComponent<SpawnerState>();

            DefenseModeConfig config = _levelService.CurrentLevel.DefenseModeConfig;

            bool allWavesCompleted = state.CurrentWaveIndex >= config.Waves.Length;

            bool allEnemiesDefeated = state.CurrentlyAlive == 0;

            if (allEnemiesDefeated && allWavesCompleted)
                _allEnemiesDefeatedEvent.NextFrame(new AllEnemiesDefeatedEvent());
        }

        public void Dispose()
        {
        }
    }
}