using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

namespace _Project.Scripts.Gameplay.Features.SaveFeature
{
    [Serializable]
    public sealed class SaveLoadService : IService
    {
        [ShowInInspector]
        private PlayerSaveData _playerSaveData;

        private float _lastAutoSaveTime;
        private const float AUTO_SAVE_INTERVAL = 300f;

        public const string PLAYER_DATA_KEY = "player_data";

        public event Action OnDataLoaded;
        public event Action OnDataSaved;
        public event Action<bool> OnSyncComplete;
        public event Action<string> OnAutoSave;

        private readonly ISaveLoadProvider _provider;
        private bool _isInitialized;

        public PlayerSaveData PlayerSaveData => _playerSaveData;
        public bool IsSyncing { get; private set; }
        public DateTime LastSaveTime { get; private set; }

        public SaveLoadService(ISaveLoadProvider provider)
        {
            _provider = provider;
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                L.LogWarning("[SaveLoadService] Already initialized. Skipping initialization.");
                return;
            }

            _provider.Initialize();
            _provider.OnSyncComplete += HandleSyncComplete;
            _provider.OnSyncError += HandleSyncError;

            LoadData();
            _lastAutoSaveTime = Time.realtimeSinceStartup;
            _isInitialized = true;
        }

        public void Update()
        {
            if (!_isInitialized)
                return;

            if (Time.realtimeSinceStartup - _lastAutoSaveTime > AUTO_SAVE_INTERVAL)
                AutoSave();

            return;

            void AutoSave()
            {
                const string REASON = "Auto save";

                SaveData(syncImmediately: true);
                _lastAutoSaveTime = Time.realtimeSinceStartup;
                OnAutoSave?.Invoke(REASON);
                L.Log($"[SaveLoadService] Auto-saved game data. Reason: {REASON}");
            }
        }

        public PlayerSaveData LoadData()
        {
            _playerSaveData = _provider.LoadData();

            if (_playerSaveData == null)
            {
                _playerSaveData = CreateDefaultPlayerData();
                L.LogWarning("[SaveLoadService] Data was null after loading, creating default data");
            }

            OnDataLoaded?.Invoke();
            return _playerSaveData;
        }

        public void SaveData(bool syncImmediately = false)
        {
            if (_playerSaveData == null)
            {
                _playerSaveData = CreateDefaultPlayerData();
                L.LogWarning("[SaveLoadService] Attempted to save null data, creating default");
            }

            IsSyncing = syncImmediately;
            _provider.SaveData(_playerSaveData, syncImmediately, success => OnSyncComplete?.Invoke(success));

            LastSaveTime = DateTime.UtcNow;
            OnDataSaved?.Invoke();
        }

        public void ResetSavesToDefault(string reason = "Manual reset")
        {
            _playerSaveData = CreateDefaultPlayerData();
            SaveData(syncImmediately: true);

            L.Log($"[SaveLoadService] Player data reset to defaults. Reason: {reason}");
        }

        private void HandleSyncComplete()
        {
            IsSyncing = false;
            L.Log("[SaveLoadService] Sync completed successfully");
            OnSyncComplete?.Invoke(true);
        }

        private void HandleSyncError()
        {
            IsSyncing = false;
            L.LogError("[SaveLoadService] Sync failed");
            OnSyncComplete?.Invoke(false);
        }

        private static PlayerSaveData CreateDefaultPlayerData() =>
            new PlayerSaveData();

#if UNITY_EDITOR
        [MenuItem("Tools/Save System/Reset Saves To Default")]
        public static void ResetSavesToDefaultFromEditor()
        {
#if GAMEPUSH_ENABLED
            ISaveLoadProvider provider1 = new GamePushSaveLoadProvider();
            PlayerSaveData defaultData1 = CreateDefaultPlayerData();
            provider1.SaveData(defaultData1, syncImmediately: false, null);
#endif
            ISaveLoadProvider provider = new MockSaveLoadProvider();
            PlayerSaveData defaultData = CreateDefaultPlayerData();
            provider.SaveData(defaultData, syncImmediately: false, null);
        }
#endif
    }
}