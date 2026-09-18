using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace _Project.Scripts.Gameplay.Features.SaveFeature
{
    [Serializable]
    public class PlayerSaveData
    {
        [Serializable]
        public class ArsenalEvolutionSaveData
        {
            [JsonProperty]
            public Dictionary<int, int> EvolutionLevels = new Dictionary<int, int>();
        }

        [Serializable]
        public class LevelSaveData
        {
            public int CurrentLevelIndex;

            [JsonProperty]
            public Dictionary<int, LevelState> LevelStates = new Dictionary<int, LevelState>();

            [Serializable]
            public class LevelState
            {
                public bool IsCompleted;
            }
        }

        [Serializable]
        public class TutorialProgressData
        {
            public List<string> CompletedTutorials = new List<string>();
            public Dictionary<string, List<int>> CompletedStepsByTemplate = new Dictionary<string, List<int>>();
            public bool TutorialCompleted;
        }

        [Serializable]
        public class EnhancementSaveData
        {
            [JsonProperty]
            public Dictionary<int, UpgradeSaveData> Upgrades = new Dictionary<int, UpgradeSaveData>();

            public class UpgradeSaveData
            {
                public int Level;
                public float CurrentValue;
            }
        }

        // Settings
        public float MusicVolume = 0.5f;
        public float SfxVolume = 0.5f;

        // Tutorial
        [JsonProperty]
        public TutorialProgressData TutorialProgress = new TutorialProgressData();

        // Arsenal data
        [JsonProperty]
        public ArsenalEvolutionSaveData ArsenalEvolutions = new ArsenalEvolutionSaveData();

        // Currency
        public int Gems;
        public int Money = 50;

        // Level
        public LevelSaveData LevelData = new LevelSaveData();

        // Enhancement
        [JsonProperty]
        public EnhancementSaveData EnhancementData = new EnhancementSaveData();
    }
}