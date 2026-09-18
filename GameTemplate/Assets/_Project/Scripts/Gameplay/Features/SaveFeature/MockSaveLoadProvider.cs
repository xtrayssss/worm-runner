using System;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using Newtonsoft.Json;
using Racer.EzSaverLite.Core;

namespace _Project.Scripts.Gameplay.Features.SaveFeature
{
    public sealed class MockSaveLoadProvider : ISaveLoadProvider
    {
        public event Action OnSyncComplete;
        public event Action OnSyncError;

        public void Initialize()
        {
        }

        public PlayerSaveData LoadData()
        {
            string jsonData = SaverManager.Saver.GetString(SaveLoadService.PLAYER_DATA_KEY);
            PlayerSaveData data = JsonConvert.DeserializeObject<PlayerSaveData>(jsonData);

            L.Log("[MockSaveLoadProvider] Loaded save data");

            return data ?? new PlayerSaveData();
        }

        public void SaveData(PlayerSaveData data, bool syncImmediately, Action<bool> onSyncComplete)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            SaverManager.Saver.SaveString(SaveLoadService.PLAYER_DATA_KEY, jsonData);
            OnSyncComplete?.Invoke();

            L.Log("[MockSaveLoadProvider] Saved data");
        }
    }
}