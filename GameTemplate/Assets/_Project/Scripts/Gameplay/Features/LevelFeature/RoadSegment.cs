using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature
{
    [Serializable]
    public class RoadSegment
    {
        [FoldoutGroup("$GetRoadLabel")]
        [ShowIf("UsePositionalPlacement")]
        [Range(2f, 6f)]
        [ShowInInspector]
        public const float LANE_WIDTH = 3f;

#if UNITY_EDITOR
        [FoldoutGroup("$GetRoadLabel")]
        [SerializeField, ReadOnly]
        public int RoadIndex;

        [FoldoutGroup("$GetRoadLabel")]
        [SerializeField, ReadOnly]
        public float StartZ;

        [FoldoutGroup("$GetRoadLabel")]
        [SerializeField, ReadOnly]
        public float EndZ;

        [FoldoutGroup("$GetRoadLabel")]
        [SerializeField, ReadOnly]
        private GameObject _view;

        [FoldoutGroup("$GetRoadLabel")]
        [Space]
        public float RoadLength = 10f;

        [FoldoutGroup("$GetRoadLabel")]
        [Range(1, 8)]
        public int ObjectCount = 3;

        [FoldoutGroup("$GetRoadLabel")]
        [Range(1.5f, 5f)]
        public float ObjectSpacing = 2.5f;

        [FoldoutGroup("$GetRoadLabel")]
        [SerializeReference]
        [ListDrawerSettings(DraggableItems = true, HideAddButton = false)]
        private ObjectGenerationPreset[] _objectPresets = Array.Empty<ObjectGenerationPreset>();

        [FoldoutGroup("$GetRoadLabel")]
        [Space]
        public bool UsePositionalPlacement = true;

        private List<SpawnPosition> _allSpawnPositions = new List<SpawnPosition>
            { SpawnPosition.LEFT, SpawnPosition.RIGHT };

        [FoldoutGroup("$GetRoadLabel")]
        [OdinSerialize, ReadOnly]
        [ShowIf(nameof(UsePositionalPlacement))]
        private HashSet<SpawnPosition> _occupiedPositions = new HashSet<SpawnPosition>(capacity: 4);

        [FoldoutGroup("$GetRoadLabel")]
        [Space]
        [SerializeField]
        private bool _isEmptyRoad;

        public bool IsEmptyRoad
        {
            get => _isEmptyRoad;
            set => _isEmptyRoad = value;
        }

        public GameObject View
        {
            get => _view;
            set => _view = value;
        }

        public ObjectGenerationPreset[] ObjectPresets
        {
            get => _objectPresets;
            set => _objectPresets = value;
        }

        public HashSet<SpawnPosition> OccupiedPositions => _occupiedPositions;

        [FoldoutGroup("$GetRoadLabel")]
        [Button("Regenerate Objects on This Road")]
        public void RegenerateObjects(InteractiveObjectId[] specificObjects)
        {
            Object activeObject = Selection.activeObject;

            if (activeObject != null)
            {
                if (activeObject is GameObject go)
                {
                    if (go.TryGetComponent(out LevelEditor levelEditor))
                    {
                        levelEditor.RegenerateRoadObjects(RoadIndex, specificObjects);
                    }
                }
            }
        }

        public ObjectGenerationPreset GetRandomPreset()
        {
            if (_objectPresets == null || _objectPresets.Length == 0)
                return null;

            float totalWeight = _objectPresets.Sum(static p => p.Weight);
            if (totalWeight <= 0f)
                return _objectPresets.FirstOrDefault();

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

        public Vector3 GetSpawnPosition(SpawnPosition spawnPos)
        {
            float roadZ = StartZ + RoadLength * Random.Range(0.5f, 0.8f);
            float xOffset = GetXOffsetForPosition(spawnPos);

            return new Vector3(xOffset, 0.1f, roadZ);
        }

        private static float GetXOffsetForPosition(SpawnPosition position)
        {
            return position switch
            {
                SpawnPosition.LEFT => -LANE_WIDTH,
                SpawnPosition.CENTER => 0f,
                SpawnPosition.RIGHT => LANE_WIDTH,
                _ => 0f
            };
        }

        public SpawnPosition? GetRandomAvailablePosition()
        {
            List<SpawnPosition> availablePositions = GetAvailablePositions();

            if (availablePositions.Count == 0)
                return null;

            int randomIndex = Random.Range(0, availablePositions.Count);
            return availablePositions[randomIndex];
        }

        public List<SpawnPosition> GetAvailablePositions() =>
            _allSpawnPositions.Where(pos => !_occupiedPositions.Contains(pos)).ToList();

        public bool OccupyPosition(SpawnPosition position) =>
            _occupiedPositions.Add(position);

        private string GetRoadLabel()
        {
            string roadType = _isEmptyRoad ? " (Empty)" : "";

            return "Road " + RoadIndex + roadType;
        }
#endif
    }
}