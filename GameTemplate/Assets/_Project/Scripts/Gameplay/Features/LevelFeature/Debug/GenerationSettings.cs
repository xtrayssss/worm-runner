#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class GenerationSettings
    {
        [TabGroup("Basic")]
        [SerializeField] private int _roadCount = 30;

        [TabGroup("Basic")]
        [SerializeField] private float _defaultRoadLength = 10f;

        [TabGroup("Basic")]
        [SerializeField] private int _defaultObjectCount = 1;

        [TabGroup("Basic")]
        [SerializeField] private float _defaultObjectSpacing = 2f;

        [TabGroup("Empty Roads")]
        [SerializeField]
        private int _emptyRoadsAtEnd = 15;

        [TabGroup("Empty Roads")]
        [SerializeField]
        private int _emptyRoadsAtStart = 1;

        public int EmptyRoadsAtStart => _emptyRoadsAtStart;

        public int EmptyRoadsAtEnd => _emptyRoadsAtEnd;

        [TabGroup("Object Presets")]
        [SerializeReference]
        [ListDrawerSettings(DraggableItems = true, HideAddButton = false)]
        private List<ObjectGenerationPreset> _objectPresets = new List<ObjectGenerationPreset>
        {
            new GateGenerationPreset(),
            new EvolutionaryIncubatorGenerationPreset(),
            new BarrelGenerationPreset(),
            new MineGenerationPreset(),
            new CannonGenerationPreset(),
            new EnemyGroupGenerationPreset(),
        };

        public int RoadCount => _roadCount;
        public float DefaultRoadLength => _defaultRoadLength;
        public int DefaultObjectCount => _defaultObjectCount;
        public float DefaultObjectSpacing => _defaultObjectSpacing;
        public List<ObjectGenerationPreset> ObjectPresets => _objectPresets;

        [TabGroup("Object Presets")]
        [Button("Apply Presets for Selected Mode", ButtonSizes.Large, DisplayParameters = false)]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        public void ApplyPresets(LevelEditor root)
        {
            LevelMode mode = root.LevelConfig != null
                ? root.LevelConfig.LevelMode
                : LevelMode.CLASSIC;

            _objectPresets.Clear();

            List<ObjectGenerationPreset> presets = mode switch
            {
                LevelMode.STEALTH => GetStealthPresets(),
                LevelMode.COLLECTOR => GetCollectorPresets(),
                LevelMode.DEFENSE => GetDefensePresets(),
                LevelMode.CAPTURE_ZONES => GetCaptureZonesPresets(),
                _ => GetClassicPresets()
            };

            _objectPresets.AddRange(presets);
        }

        private List<ObjectGenerationPreset> GetCaptureZonesPresets()
        {
            List<ObjectGenerationPreset> defaultPresets = GetClassicPresets();

            return defaultPresets;
        }

        private List<ObjectGenerationPreset> GetDefensePresets()
        {
            return new List<ObjectGenerationPreset>
            {
                new GateGenerationPreset(),
                new BubbleGenerationPreset()
            };
        }

        private List<ObjectGenerationPreset> GetClassicPresets()
        {
            return new List<ObjectGenerationPreset>
            {
                new GateGenerationPreset(),
                new EvolutionaryIncubatorGenerationPreset(),
                new BarrelGenerationPreset(),
                new MineGenerationPreset(),
                new CannonGenerationPreset(),
                new EnemyEmplacementGenerationPreset(),
                new EnemyGroupGenerationPreset(),
                new DamagedTankGenerationPreset(),
                new MachineGunnerGenerationPreset(),
            };
        }

        private List<ObjectGenerationPreset> GetCollectorPresets()
        {
            List<ObjectGenerationPreset> defaultPresets = GetClassicPresets();

            return defaultPresets;
        }

        private List<ObjectGenerationPreset> GetStealthPresets()
        {
            return new List<ObjectGenerationPreset>
            {
                new WatchTowerGenerationPreset(),
                new PatrolGuardGenerationPreset(),
                new MineGenerationPreset()
            };
        }

        public ObjectGenerationPreset GetRandomPreset()
        {
            if (_objectPresets == null || _objectPresets.Count == 0)
                return null;

            float totalWeight = _objectPresets.Sum(static p => p.Weight);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (ObjectGenerationPreset preset in _objectPresets)
            {
                currentWeight += preset.Weight;
                if (randomValue <= currentWeight)
                    return preset;
            }

            return _objectPresets.LastOrDefault();
        }

        public ObjectGenerationPreset GetPresetForType(InteractiveObjectId objectType) =>
            _objectPresets?.FirstOrDefault(p => p.ObjectType == objectType);
    }
}
#endif