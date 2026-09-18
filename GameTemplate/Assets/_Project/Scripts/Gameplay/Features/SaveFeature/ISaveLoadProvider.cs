using System;

namespace _Project.Scripts.Gameplay.Features.SaveFeature
{
    public interface ISaveLoadProvider
    {
        public void Initialize();
        public PlayerSaveData LoadData();
        public void SaveData(PlayerSaveData data, bool syncImmediately, Action<bool> onSyncComplete);
        public event Action OnSyncComplete;
        public event Action OnSyncError;
    }
}