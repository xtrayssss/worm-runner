using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [CreateAssetMenu(fileName = "Chapter_", menuName = ProjectConfig.PROJECT_NAME + "/Configs/Chapter Config")]
    public class ChapterConfig : ScriptableObject
    {
        [SerializeField]
        private BiomeData _biome;

        [SerializeField]
        [ListDrawerSettings(
            DraggableItems = true,
            HideAddButton = false,
            HideRemoveButton = false,
            NumberOfItemsPerPage = 20
        )]
        private List<LevelConfig> _levels;

        public BiomeData Biome => _biome;
        public IReadOnlyList<LevelConfig> Levels => _levels;
        public int LevelsCount => _levels.Count;

        [Serializable]
        public class BiomeData
        {
            [SerializeField]
            private Color _fogColor = Color.white;

            [SerializeField]
            private float _fogParticleAlpha = 100f;

            [SerializeField]
            private Color _roadColor = Color.gray;

            [SerializeField]
            private Sprite _groundSprite;

            public Color FogColor => _fogColor;
            public Color RoadColor => _roadColor;
            public Sprite GroundSprite => _groundSprite;
            public float FogParticleAlpha => _fogParticleAlpha;
        }
    }
}