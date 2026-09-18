#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class EnemyGroupGenerationPreset : ObjectGenerationPreset
    {
        [TabGroup("Enemy Group Settings")]
        [SerializeField]
        private bool _useRandomFormation = true;

        [TabGroup("Enemy Group Settings")]
        [ShowIf("@!_useRandomFormation")]
        [SerializeField]
        private EnemyFormationType _formationType = EnemyFormationType.LINE_THREE;

        [TabGroup("Enemy Group Settings")]
        [SerializeField]
        [Range(10f, 200f)]
        private Vector2 _healthRange = new Vector2(30f, 80f);

        [TabGroup("Enemy Group Settings")]
        [SerializeField]
        [Range(5f, 50f)]
        private Vector2 _damageRange = new Vector2(10f, 20f);

        [TabGroup("Enemy Group Settings")]
        [SerializeField]
        [Range(0.5f, 5f)]
        private float _shootingCooldown = 2f;

        [TabGroup("Enemy Group Settings")]
        [SerializeField]
        [Range(1, 50)]
        private Vector2Int _moneyRewardRange = new Vector2Int(5, 15);

        public override InteractiveObjectId ObjectType => InteractiveObjectId.ENEMY_GROUP;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            config.EnemyGroupConfig = new EnemyGroupConfig
            {
                FormationType = GetRandomFormation()
            };

            // EnemyGroup enemyGroup = new EnemyGroup
            // {
            //     Position = position,
            //     RoadIndex = roadIndex,
            //     IsActive = true
            // };
            //
            // float health = UnityEngine.Random.Range(_healthRange.x, _healthRange.y);
            // float damage = UnityEngine.Random.Range(_damageRange.x, _damageRange.y);
            // int money = UnityEngine.Random.Range(_moneyRewardRange.x, _moneyRewardRange.y);
            //
            // enemyGroup.GenerateObjects();
        }

        public static EnemyFormationType GetRandomFormation()
        {
            Array values = Enum.GetValues(typeof(EnemyFormationType));
            return (EnemyFormationType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }
}
#endif