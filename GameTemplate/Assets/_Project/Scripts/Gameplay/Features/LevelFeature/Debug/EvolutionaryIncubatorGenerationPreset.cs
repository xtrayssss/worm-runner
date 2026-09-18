#if UNITY_EDITOR
using System;
using System.Linq;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class EvolutionaryIncubatorGenerationPreset : ObjectGenerationPreset
    {
        [TabGroup("Evolutionary Incubator Settings")]
        [SerializeField] private bool _isEnhanceable = true;

        [TabGroup("Evolutionary Incubator Settings")]
        [SerializeField] private float[] _spawnCountChances =
        {
            0.6f,
            0.3f,
            0.2f,
            0.15f,
            0.05f
        };

        public int GetRandomCount()
        {
            float totalWeight = _spawnCountChances.Sum();

            if (totalWeight <= 0)
                return 1;

            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            for (int i = 0; i < _spawnCountChances.Length; i++)
            {
                cumulativeWeight += _spawnCountChances[i];

                if (randomValue < cumulativeWeight)
                    return i + 1;
            }

            return 5;
        }

        public override InteractiveObjectId ObjectType => InteractiveObjectId.EVOLUTIONARY_INCUBATOR;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            EvolutionaryIncubatorConfig incubatorConfig = new EvolutionaryIncubatorConfig
            {
                SpawnCount = GetRandomCount(),
                IsEnhanceable = _isEnhanceable
            };

            config.EvolutionaryIncubatorConfig = incubatorConfig;
        }
    }
}
#endif