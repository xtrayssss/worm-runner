#if GAMEPUSH_ENABLED
using System;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
using GamePush;
using Newtonsoft.Json;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.SaveFeature
{
    public sealed class GamePushSaveLoadProvider : ISaveLoadProvider
    {
        public event Action OnSyncComplete;
        public event Action OnSyncError;

        public void Initialize()
        {
            GP_Player.OnSyncComplete += HandleSyncComplete;
            GP_Player.OnSyncError += HandleSyncError;
            
            L.Log("[GamePushSaveLoadProvider] Initialized");
        }

        public PlayerSaveData LoadData()
        {
            string jsonData = GP_Player.GetString(SaveLoadService.PLAYER_DATA_KEY);

            if (string.IsNullOrEmpty(jsonData))
            {
                L.Log("[GamePushSaveLoadProvider] No data found in GamePush, creating default data");
                return new PlayerSaveData();
            }

            PlayerSaveData data = JsonConvert.DeserializeObject<PlayerSaveData>(jsonData);
            
            L.Log("[GamePushSaveLoadProvider] Successfully loaded save data from GamePush");
            return data;
        }

        public void SaveData(PlayerSaveData data, bool syncImmediately, Action<bool> onSyncComplete)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            GP_Player.Set(SaveLoadService.PLAYER_DATA_KEY, jsonData);

            GP_Player.Sync();
        }

        private void HandleSyncComplete()
        {
            L.Log("[GamePushSaveLoadProvider] Sync completed successfully");
            OnSyncComplete?.Invoke();
        }

        private void HandleSyncError()
        {
            L.LogError("[GamePushSaveLoadProvider] Sync failed");
            OnSyncError?.Invoke();
        }
    }
}

#endif