using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LoadingScreenFeature;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using _Project.Scripts.Gameplay.Features.SaveFeature;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEditor.Searcher;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Services
{
    [Serializable]
    public sealed class LevelService : IService
    {
        [ShowInInspector]
        private LevelConfig _currentLevel;

        [ShowInInspector]
        [ListDrawerSettings(DefaultExpandedState = false)]
        private readonly List<Entity> _currentLevelObjects = new List<Entity>();

        [ShowInInspector]
        private LevelView _currentLevelView;

        [ShowInInspector]
        public float LevelTime { get; set; }

        [ShowInInspector]
        public int CurrentLevelIndex
        {
            get => _saveLoadService.PlayerSaveData.LevelData.CurrentLevelIndex;
            private set
            {
                _saveLoadService.PlayerSaveData.LevelData.CurrentLevelIndex = value;
                _saveLoadService.SaveData();
            }
        }

        public event Action<int> OnLevelChanged;
        public LevelView CurrentLevelView => _currentLevelView;
        public LevelConfig CurrentLevel => _currentLevel;
        public bool[] StrikesTriggered { get; private set; } = Array.Empty<bool>();

        private World _world;

        private readonly ConfigsService _configs;
        private readonly InteractiveObjectFactory _interactiveObjectFactory;
        private readonly SaveLoadService _saveLoadService;
        private readonly CrowdFactory _crowdFactory;
        private readonly CollectibleService _collectibleService;
        private readonly EnhancementService _enhancementService;
        private readonly CaptureZoneService _captureZoneService;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;
        private readonly UIRoot _uiRoot;

        public LevelService(ConfigsService configs,
            InteractiveObjectFactory interactiveObjectFactory,
            SaveLoadService saveLoadService,
            EnhancementService enhancementService,
            CrowdFactory crowdFactory,
            CollectibleService collectibleService,
            CaptureZoneService captureZoneService,
            SceneLoader sceneLoader,
            LoadingCurtain loadingCurtain,
            UIRoot uiRoot)
        {
            _configs = configs;
            _interactiveObjectFactory = interactiveObjectFactory;
            _saveLoadService = saveLoadService;
            _enhancementService = enhancementService;
            _crowdFactory = crowdFactory;
            _collectibleService = collectibleService;
            _captureZoneService = captureZoneService;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _uiRoot = uiRoot;
            _world = World.Default;
        }

        public void Initialize() =>
            LoadSaveData();

        public void LoadSaveData()
        {
            LevelsDatabase database = _configs.GetLevelsDatabase();

            for (int i = 0; i < database.GetLevelsCount(); i++)
                GetLevelState(i);
        }

        public PlayerSaveData.LevelSaveData.LevelState GetLevelState(int levelIndex)
        {
            PlayerSaveData saveData = _saveLoadService.PlayerSaveData;

            if (!saveData.LevelData.LevelStates.ContainsKey(levelIndex))
                saveData.LevelData.LevelStates[levelIndex] = new PlayerSaveData.LevelSaveData.LevelState();

            return saveData.LevelData.LevelStates[levelIndex];
        }

        private async UniTask LoadLevel(int levelIndex, bool withFadeAnimation)
        {
            await _loadingCurtain.ShowAsync(withFadeAnimation);
            await _sceneLoader.Load(InfrastructureAssetPath.GAMEPLAY_SCENE);

            LevelsDatabase database = _configs.GetLevelsDatabase();
            _currentLevel = database.GetLevel(levelIndex);

            CurrentLevelIndex = levelIndex;

            StrikesTriggered = _currentLevel.MortarStrikes.Length > 0
                ? new bool[_currentLevel.MortarStrikes.Length]
                : Array.Empty<bool>();

            _saveLoadService.PlayerSaveData.LevelData.CurrentLevelIndex = levelIndex;

            SetLevelCompleted(levelIndex, false);

            CreateLevel();

            InitializeLevelMode();

            CreateCrowdForLevel();

            OnLevelChanged?.Invoke(CurrentLevelIndex);

            _loadingCurtain.Hide();
        }

        public void AdvanceToNextLevel()
        {
            int nextLevelIndex = CurrentLevelIndex + 1;
            LevelsDatabase database = _configs.GetLevelsDatabase();

            if (nextLevelIndex >= database.GetLevelsCount())
            {
                L.LogWarning("No more levels available!");
                return;
            }

            CurrentLevelIndex = nextLevelIndex;
        }

        public void SetLevelCompleted(int levelIndex, bool completed = true)
        {
            PlayerSaveData.LevelSaveData.LevelState levelState = GetLevelState(levelIndex);

            levelState.IsCompleted = completed;

            _saveLoadService.SaveData();
        }

        public bool IsLevelCompleted(int levelIndex) =>
            GetLevelState(levelIndex).IsCompleted;

        public bool IsNewChapter(int globalLevelIndex)
        {
            LevelsDatabase database = _configs.GetLevelsDatabase();
            ChapterConfig chapter = database.GetChapterForLevel(globalLevelIndex, out int localIndex);
            return chapter != null && localIndex == 0 && globalLevelIndex != 0;
        }

        public async UniTask LoadNextLevel()
        {
            AdvanceToNextLevel();
            await LoadLevel(CurrentLevelIndex, withFadeAnimation: true);
        }

        public async UniTask ReloadCurrentLevel(bool withFadeAnimation)
        {
            await LoadLevel(CurrentLevelIndex, withFadeAnimation);
        }

        public bool HasNextLevel()
        {
            LevelsDatabase database = _configs.GetLevelsDatabase();
            return CurrentLevelIndex + 1 < database.GetLevelsCount();
        }

        public float GetCurrentLevelDistance()
        {
            LevelObjectData finishTrigger =
                _currentLevel.LevelObjects.Find(static obj => obj.ObjectType == InteractiveObjectId.FINISH_TRIGGER);

            if (finishTrigger == null)
                return 0f;

            return finishTrigger.Position.z;
        }

        private void CreateLevel()
        {
#if UNITY_EDITOR
            _currentLevel.LevelPrefab.gameObject.SetActive(false);
#endif
            _currentLevelView = Object.Instantiate(_currentLevel.LevelPrefab);

            _currentLevelView.Construct(_uiRoot);

            LevelsDatabase database = _configs.GetLevelsDatabase();

            ChapterConfig chapter = database.GetChapterForLevel(CurrentLevelIndex, out _);

            _currentLevelView.ApplyBiome(chapter.Biome);

#if UNITY_EDITOR
            RemoveEditorOnlyObjects(_currentLevelView.transform);

            _currentLevel.LevelPrefab.gameObject.SetActive(true);
            _currentLevelView.gameObject.SetActive(true);
#endif
            foreach (LevelObjectData objectData in _currentLevel.LevelObjects)
            {
                if (!objectData.IsActive)
                    continue;

                switch (objectData)
                {
                    case PostFinishFarmingArea farmingArea:
                    {
                        foreach (LevelObjectData stoneData in farmingArea.CashStones)
                        {
                            Entity stone = CreateLevelObject(stoneData);
                            _currentLevelObjects.Add(stone);
                        }

                        break;
                    }
                    case EnemyGroup enemyGroup:
                    {
                        Entity[] enemies =
                            _interactiveObjectFactory.CreateEnemyGroup(enemyGroup, _currentLevelView.transform);
                        _currentLevelObjects.AddRange(enemies);
                        break;
                    }
                    case not null when objectData.CustomData.IsEnemy():
                    {
                        Entity createdEntity = CreateLevelObject(objectData);
                        ref readonly EntityViewLink viewLink = ref createdEntity.GetComponent<EntityViewLink>();
                        viewLink.View.transform.position += Vector3.up * 1.2f;
                        break;
                    }
                    default:
                    {
                        Entity createdEntity = CreateLevelObject(objectData);
                        _currentLevelObjects.Add(createdEntity);
                        break;
                    }
                }
            }
        }

        private void InitializeLevelMode()
        {
            switch (_currentLevel.LevelMode)
            {
                case LevelMode.COLLECTOR:
                    InitializeCollectorMode();
                    break;
                case LevelMode.DEFENSE:
                    InitializeDefenseMode();
                    break;
                case LevelMode.CAPTURE_ZONES:
                    InitializeCaptureZonesMode();
                    break;
            }

            return;

            void InitializeCaptureZonesMode()
            {
                _captureZoneService.Reset();
                _captureZoneService.Setup(requiredPercent: 0.7f);
            }

            void InitializeCollectorMode()
            {
                _collectibleService.Reset();
                _collectibleService.Setup();
            }

            void InitializeDefenseMode()
            {
                Entity spawnEntity = _world.CreateEntity();

                DefenseModeConfig defenseModeConfig = _currentLevel.DefenseModeConfig;

                spawnEntity.AddComponent<SpawnerState>() = new SpawnerState
                {
                    CurrentWaveIndex = 0,
                    WaveTimer = 0f,
                    CurrentEnemyIndex = 0,
                    IsWaveActive = defenseModeConfig.Waves.Length > 0 &&
                                   defenseModeConfig.Waves[0].DelayBeforeStart <= 0,

                    IsWaveDelayActive = defenseModeConfig.Waves.Length > 0 &&
                                        defenseModeConfig.Waves[0].DelayBeforeStart > 0,

                    DelayTimer = defenseModeConfig.Waves.Length > 0 ? defenseModeConfig.Waves[0].DelayBeforeStart : 0f
                };
            }
        }

#if UNITY_EDITOR
        private void RemoveEditorOnlyObjects(Transform levelTransform)
        {
            List<Transform> editorOnlyObjects = new List<Transform>();

            FindEditorOnlyObjects(levelTransform);

            foreach (Transform editorObj in editorOnlyObjects)
            {
                Object.Destroy(editorObj.gameObject);
            }

            return;

            void FindEditorOnlyObjects(Transform parent)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);

                    if (child.gameObject.CompareTag("EditorOnly"))
                        editorOnlyObjects.Add(child);
                    else
                        FindEditorOnlyObjects(child);
                }
            }
        }
#endif

        private Entity CreateLevelObject(LevelObjectData objectData) =>
            _interactiveObjectFactory.CreateInteractiveObject(objectData, _currentLevelView.transform);

        private void CreateCrowdForLevel()
        {
            _crowdFactory.CreateCrowd();

            int population = _currentLevel.LevelMode == LevelMode.STEALTH
                ? 1
                : (int)_enhancementService.Upgrades[UpgradeType.POPULATION].CurrentValue;

            int evolutionLevel = (int)_enhancementService.Upgrades[UpgradeType.EVOLUTION].CurrentValue;

            World.Default
                .GetRequest<AddCrowdMembersRequest>()
                .Publish(new AddCrowdMembersRequest
                {
                    MemberType = CrowdMemberType.MILITARY,
                    Count = population,
                    EvolutionLevel = evolutionLevel,
                    AnimationType = CrowdMemberAnimationType.IDLE
                }, allowNextFrame: true);
        }

        public void ResetLevelTime() =>
            LevelTime = 0f;
    }
}