using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CurrencyFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [Serializable]
    public class PostFinishFarmingArea : LevelObjectData
    {
#if UNITY_EDITOR
        [FoldoutGroup("Header/$ObjectTitle")]
        [SerializeField]
        private int _gridHeight = 5;

        [FoldoutGroup("Header/$ObjectTitle")]
        [SerializeField]
        private float _stoneSpacing = 4f;

        [FoldoutGroup("Header/$ObjectTitle")]
        [SerializeField]
        private Vector3 _areaOffset = new Vector3(0, 0, 0);
#endif

        [FoldoutGroup("Header/$ObjectTitle")]
        [ReadOnly]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [NonSerialized, OdinSerialize]
        private List<LevelObjectData> _cashStones = new List<LevelObjectData>();

        public List<LevelObjectData> CashStones => _cashStones;

#if UNITY_EDITOR
        [FoldoutGroup("Header/$ObjectTitle")]
        [Button(DisplayParameters = false)]
        public void GenerateObjects([CanBeNull] object root = null)
        {
            LevelEditor levelEditor = (LevelEditor)root;

            foreach (LevelObjectData stone in _cashStones)
            {
                if (stone.View != null)
                    Object.DestroyImmediate(stone.View.gameObject);
            }

            _cashStones.Clear();

            Vector3 basePosition = Position + _areaOffset;

            for (int row = 0; row < _gridHeight; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    Vector3 stonePosition = basePosition + new Vector3(
                        GetXOffsetForPosition((SpawnPosition)col),
                        0.1f,
                        row * _stoneSpacing);

                    CurrencyReward moneyReward = new CurrencyReward(CurrencyType.MONEY, 5 * (row + 1));

                    LevelObjectData cashStoneData = ObjectGenerator.GenerateObject<LevelObjectData>(
                        id: InteractiveObjectId.CASH_STONE,
                        position: stonePosition,
                        roadIndex: RoadIndex,
                        customData: new InteractiveObjectData
                        {
                            Rewards = new Rewards(moneyReward),
                            MaxHealth = CalculateHealthForRow(row)
                        });

                    _cashStones.Add(cashStoneData);

                    if (levelEditor != null)
                        levelEditor.CreateObjectView(cashStoneData, View.transform);
                }
            }
        }

        private int CalculateHealthForRow(int rowIndex)
        {
            const int BASE_HEALTH = 120;
            const int HEALTH_INCREMENT = 100;
            return BASE_HEALTH + rowIndex * HEALTH_INCREMENT;
        }

        private float GetXOffsetForPosition(SpawnPosition position)
        {
            float xOffsetForPosition = position switch
            {
                SpawnPosition.LEFT => -RoadSegment.LANE_WIDTH - RoadSegment.LANE_WIDTH * 0.3f,
                SpawnPosition.CENTER => 0f,
                SpawnPosition.RIGHT => RoadSegment.LANE_WIDTH + RoadSegment.LANE_WIDTH * 0.3f,
                _ => 0f
            };

            return xOffsetForPosition + (int)position * 0f;
        }

        public Vector3 CalculateAreaSize()
        {
            float width = RoadSegment.LANE_WIDTH;
            float length = (_gridHeight - 1) * _stoneSpacing;
            float height = 2f;

            return new Vector3(width, height, length);
        }
#endif
    }
}