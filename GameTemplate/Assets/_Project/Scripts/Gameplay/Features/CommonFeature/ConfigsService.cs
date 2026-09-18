using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.ConfettiFeature;
using _Project.Scripts.Gameplay.Features.ConfettiFeature.Configs;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Configs;
using _Project.Scripts.Gameplay.Features.DamageFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Configs;
using _Project.Scripts.Gameplay.Features.GameFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.ProjectileFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Configs;
using _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature.Configs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.CommonFeature
{
    [Serializable]
    public sealed class ConfigsService : IService
    {
        private readonly Dictionary<WindowId, BaseWindow> _windowPrefabs = new Dictionary<WindowId, BaseWindow>();
        private Dictionary<ConfettiId, ConfettiConfig> _confettiConfigs = new Dictionary<ConfettiId, ConfettiConfig>();

        // Individual configs
        private GameConfig _gameConfig;
        private WindowsConfig _windowsConfig;

        // Prefabs with consistent naming
        private DamagePopupView _damagePopupPrefab;
        private AudioService _gameAudioPrefab;
        private RewardCard _fragmentRewardCard;
        private RewardCard _currencyRewardCard;
        private CrowdConfig _crowdConfig;
        private LevelsDatabase _levelsDatabase;

        private Dictionary<InteractiveObjectId, InteractiveObjectConfig> _interactiveObjectConfigs =
            new Dictionary<InteractiveObjectId, InteractiveObjectConfig>();

        private Dictionary<ProjectileId, ProjectileConfig> _projectileConfigs =
            new Dictionary<ProjectileId, ProjectileConfig>();

        private Dictionary<UpgradeType, UpgradeConfig> _upgradeConfigs =
            new Dictionary<UpgradeType, UpgradeConfig>();

        private SoulView _soulPrefab;
        private DecorativeMemberView _decorativeMemberPrefab;
        private MortarTelegraphView _mortarTelegraphPrefab;

        public void Initialize()
        {
            LoadAllConfigs();
            LoadAllPrefabs();
            InitializeWindowPrefabs();
        }

        private void LoadAllConfigs()
        {
            // Load dictionary-based configs
            // LoadConfigDictionary(
            //     "Confetti",
            //     ref _confettiConfigs,
            //     static config => config.Id);

            LoadConfigDictionary(
                "Configs/Projectiles",
                ref _projectileConfigs,
                static config => config.ProjectileId);

            LoadConfigDictionary(
                "Configs/Upgrades",
                ref _upgradeConfigs,
                static config => config.Type);

            LoadConfigDictionary(
                "Configs",
                ref _interactiveObjectConfigs,
                static config => config.InteractiveObjectData.Id);

            // Load individual configs
            _gameConfig = LoadConfig<GameConfig>("Configs/GameConfig");
            _windowsConfig = LoadConfig<WindowsConfig>("Configs/Windows/WindowsConfig");
            _crowdConfig = LoadConfig<CrowdConfig>($"Configs/{nameof(CrowdConfig)}");
            _levelsDatabase = LoadConfig<LevelsDatabase>("Configs/Levels/LevelsDatabase");
        }

        private void LoadAllPrefabs()
        {
            // Load all prefabs using consistent paths
            _damagePopupPrefab = LoadPrefab<DamagePopupView>("Prefabs/DamagePopup");
            _gameAudioPrefab = LoadPrefab<AudioService>("GameAudio");
            _soulPrefab = LoadPrefab<SoulView>("Prefabs/Soul");
            _decorativeMemberPrefab = LoadPrefab<DecorativeMemberView>("Prefabs/DecorativeMember");
            _mortarTelegraphPrefab = LoadPrefab<MortarTelegraphView>("Prefabs/MortarTelegraph");
        }

        private void InitializeWindowPrefabs()
        {
            foreach (var windowConfig in _windowsConfig.WindowConfigs)
            {
                _windowPrefabs[windowConfig.Id] = windowConfig.Prefab;
            }
        }

        #region Helper Methods

        private void LoadConfigDictionary<TConfig, TKey>(
            string path,
            ref Dictionary<TKey, TConfig> dictionary,
            Func<TConfig, TKey> keySelector) where TConfig : ScriptableObject
        {
            var configs = Resources.LoadAll<TConfig>(path);
            if (configs == null || configs.Length == 0)
            {
#if DEBUG
                Debug.LogWarning($"No configs found at path: {path}");
#endif
                return;
            }

            foreach (var config in configs)
            {
                var key = keySelector(config);
                dictionary[key] = config;
            }
        }

        private T LoadConfig<T>(string path) where T : ScriptableObject
        {
            var config = Resources.Load<T>(path);
            if (config == null)
            {
#if DEBUG
                Debug.LogError($"Failed to load config at path: {path}");
#endif
            }

            return config;
        }

        private T LoadPrefab<T>(string path) where T : Object
        {
            var prefab = Resources.Load<T>(path);
            if (prefab == null)
            {
#if DEBUG
                Debug.LogError($"Failed to load prefab at path: {path}");
#endif
            }

            return prefab;
        }

        #endregion

        #region Public Config Getters

        public CrowdConfig GetCrowdConfig() => _crowdConfig;
        public GameConfig GetGameConfig() => _gameConfig;

        public LevelsDatabase GetLevelsDatabase() =>
            _levelsDatabase;

        public InteractiveObjectConfig GetInteractiveObjectConfig(InteractiveObjectId id) =>
            GetConfigSafe(_interactiveObjectConfigs, id, "interactive object");

        public ProjectileConfig GetProjectileConfig(ProjectileId id) =>
            GetConfigSafe(_projectileConfigs, id, "interactive object");

        public UpgradeConfig GetUpgradeConfigs(UpgradeType type) =>
            GetConfigSafe(_upgradeConfigs, type, "upgrade");

        public Dictionary<UpgradeType, UpgradeConfig>.ValueCollection GetAllUpgradeConfigs() =>
            _upgradeConfigs.Values;

        #endregion

        #region Public Collection Getters

        public Dictionary<ConfettiId, ConfettiConfig> GetConfettiConfigs() =>
            _confettiConfigs;

        public Dictionary<WindowId, BaseWindow> GetWindowPrefabs() =>
            _windowPrefabs;

        #endregion

        #region Public Prefab Getters

        public DamagePopupView GetDamagePopupPrefab() => _damagePopupPrefab;

        public AudioService GetGameAudioPrefab() =>
            _gameAudioPrefab;

        public RewardCard GetFragmentRewardCard() =>
            _fragmentRewardCard;

        public RewardCard GetCurrencyRewardCard() =>
            _currencyRewardCard;

        public SoulView GetSoulPrefab() =>
            _soulPrefab;

        public DecorativeMemberView GetDecorativeMemberPrefab() =>
            _decorativeMemberPrefab;

        public MortarTelegraphView GetMortarTelegraphPrefab() =>
            _mortarTelegraphPrefab;

        #endregion

        #region Private Utility Methods

        private T GetConfigSafe<T, TKey>(Dictionary<TKey, T> configs, TKey key, string configType)
        {
            if (!configs.TryGetValue(key, out var config))
            {
#if DEBUG
                Debug.LogError($"Failed to find {configType} config for key: {key}");
#endif
                return default;
            }

            return config;
        }

        #endregion
    }
}