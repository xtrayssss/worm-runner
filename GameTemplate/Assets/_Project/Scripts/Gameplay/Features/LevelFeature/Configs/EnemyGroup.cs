using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Configs
{
    [Serializable]
    public class EnemyGroup : LevelObjectData
    {
#if UNITY_EDITOR
        [FoldoutGroup("Header/$ObjectTitle")]
        [SerializeField]
        [Range(5f, 50f)]
        private float _damage = 10f;

        [FoldoutGroup("Header/$ObjectTitle")]
        [SerializeField]
        [Range(0.5f, 5f)]
        private float _shootingCooldown = 2f;

        [FoldoutGroup("Header/$ObjectTitle")]
        [SerializeField]
        private int _moneyReward = 5;

        [FoldoutGroup("Header/$ObjectTitle")]
        [Button("Regenerate Formation", DisplayParameters = false)]
        [PropertyOrder(100)]
        private void RegenerateFormation(UnityEngine.Object root)
        {
            if (root is not LevelEditor levelEditor)
                return;

            CustomData.EnemyGroupConfig.FormationType = EnemyGroupGenerationPreset.GetRandomFormation();
            GenerateEnemies(levelEditor);
            View.name = $"EnemyGroup_{CustomData.EnemyGroupConfig.FormationType}";
        }
#endif

        [FoldoutGroup("Header/$ObjectTitle")]
        [ReadOnly]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [NonSerialized, OdinSerialize]
        private List<LevelObjectData> _enemies = new List<LevelObjectData>();

        public List<LevelObjectData> Enemies => _enemies;

#if UNITY_EDITOR
        public void GenerateEnemies(LevelEditor levelEditor)
        {
            foreach (LevelObjectData enemyData in _enemies)
            {
                if (enemyData.View != null)
                    UnityEngine.Object.DestroyImmediate(enemyData.View.gameObject);
            }

            _enemies.Clear();

            EnemyGroupView view = View.GetComponent<EnemyGroupView>();

            Vector3[] formationPositions = GetFormationPositions(CustomData.EnemyGroupConfig.FormationType, view);

            foreach (Vector3 localOffset in formationPositions)
            {
                LevelObjectData enemyData = ObjectGenerator.GenerateObject<LevelObjectData>(
                    id: InteractiveObjectId.RIFLEMAN_ENEMY,
                    position: Vector3.zero,
                    roadIndex: RoadIndex,
                    customData: new InteractiveObjectData
                    {
                        EnemyConfig = new EnemyConfig
                        {
                            RiflemanEnemyConfig = new RiflemanEnemyConfig()
                        }
                    }
                );

                InteractiveObjectView enemyView = levelEditor.CreateObjectView(enemyData, View.transform);
                enemyView.transform.localPosition = localOffset;

                _enemies.Add(enemyData);
            }
        }

        private static Vector3[] GetFormationPositions(EnemyFormationType formationType, EnemyGroupView view) =>
            view.SpawnPoints[formationType].Select(static p => p.localPosition).ToArray();
#endif
    }
}