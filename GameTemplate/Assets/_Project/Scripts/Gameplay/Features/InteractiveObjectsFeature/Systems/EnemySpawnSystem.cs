using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using PrimeTween;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnemySpawnSystem : ISystem
    {
        public World World { get; set; }

        private readonly LevelService _levelService;
        private readonly InteractiveObjectFactory _interactiveObjectFactory;
        private readonly GameStateMachine _gameStateMachine;

        private Filter _spawners;
        private Filter _enemies;

        public EnemySpawnSystem(
            LevelService levelService,
            InteractiveObjectFactory interactiveObjectFactory,
            GameStateMachine gameStateMachine)
        {
            _levelService = levelService;
            _interactiveObjectFactory = interactiveObjectFactory;
            _gameStateMachine = gameStateMachine;
        }

        public void OnAwake()
        {
            _spawners = World.Filter
                .With<SpawnerState>()
                .Build();
            
            _enemies = World.Filter
                .With<EnemyTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState ||
                _levelService.CurrentLevel.LevelMode != LevelMode.DEFENSE)
                return;

            Entity spawner = _spawners.First();
            ref SpawnerState state = ref spawner.GetComponent<SpawnerState>();
            
            state.CurrentlyAlive = _enemies.GetLengthSlow();

            DefenseModeConfig config = _levelService.CurrentLevel.DefenseModeConfig;

            if (config.Waves == null || config.Waves.Length == 0)
                return;

            if (state.CurrentWaveIndex >= config.Waves.Length)
                return;

            if (state.IsWaveDelayActive)
            {
                state.DelayTimer -= deltaTime;

                if (state.DelayTimer <= 0)
                {
                    state.IsWaveDelayActive = false;
                    state.IsWaveActive = true;
                    state.WaveTimer = 0f;
                    state.CurrentEnemyIndex = 0;
                }

                return;
            }

            if (!state.IsWaveActive)
                return;

            Wave currentWave = config.Waves[state.CurrentWaveIndex];
            state.WaveTimer += deltaTime;

            while (state.CurrentEnemyIndex < currentWave.Enemies.Length)
            {
                EnemySpawnData spawnData = currentWave.Enemies[state.CurrentEnemyIndex];

                if (state.WaveTimer >= spawnData.SpawnTime)
                {
                    SpawnEnemy(spawnData, config);
                    state.CurrentEnemyIndex++;
                }
                else
                {
                    break;
                }
            }

            if (state.CurrentEnemyIndex >= currentWave.Enemies.Length)
            {
                if (state.CurrentlyAlive == 0)
                {
                    state.CurrentWaveIndex++;
                    state.IsWaveActive = false;
            
                    if (state.CurrentWaveIndex < config.Waves.Length)
                    {
                        state.IsWaveDelayActive = true;
                        state.DelayTimer = config.Waves[state.CurrentWaveIndex].DelayBeforeStart;
                    }
                }
            }
        }

        private void SpawnEnemy(EnemySpawnData spawnData, DefenseModeConfig config)
        {
            Vector3 spawnPosition = CalculateSpawnPosition(config);

            LevelObjectData enemyLevelObject = ObjectGenerator.GenerateObject<LevelObjectData>(
                id: spawnData.Type,
                spawnPosition,
                customData: new InteractiveObjectData
                {
                    EnemyConfig = new EnemyConfig
                    {
                        RiflemanEnemyConfig = spawnData.Type == InteractiveObjectId.RIFLEMAN_ENEMY
                            ? new RiflemanEnemyConfig()
                            : null,

                        KamikazeEnemyConfig = spawnData.Type == InteractiveObjectId.KAMIKAZE_ENEMY
                            ? new KamikazeEnemyConfig()
                            : null,

                        MachineGunnerEnemyConfig = spawnData.Type == InteractiveObjectId.MACHINE_GUNNER_ENEMY
                            ? new MachineGunnerEnemyConfig()
                            : null
                    }
                });

            Entity enemy = _interactiveObjectFactory.CreateInteractiveObject(
                enemyLevelObject,
                _levelService.CurrentLevelView.transform);

            ref readonly EntityViewLink viewLink = ref enemy.GetComponent<EntityViewLink>();
            EnemyView enemyView = (EnemyView)viewLink.View;

            enemyView.transform.position += Vector3.up * 1.2f;
            enemyView.StartJump();

            PlaySpawnAnimation(enemyView);
        }

        private static void PlaySpawnAnimation(EnemyView enemyView)
        {
            const float DURATION = 0.3f;

            Tween.PunchScale(
                enemyView.transform,
                new Vector3(enemyView.transform.localScale.x * 0.25f, enemyView.transform.localScale.y * 0.25f, 0),
                DURATION,
                frequency: 1,
                easeBetweenShakes: Ease.OutElastic);
        }

        private Vector3 CalculateSpawnPosition(DefenseModeConfig config)
        {
            Vector3 basePosition = config.SpawnAreaCenter;
            Vector3 offset = Vector3.zero;

            offset.x = Random.Range(-4.5f, 4.5f);
            offset.z = Random.Range(-25f, 25f);

            return basePosition + offset;
        }

        public void Dispose()
        {
        }
    }
}