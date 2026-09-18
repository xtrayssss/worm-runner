using System;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record EnemyConfig : BaseInteractiveObjectConfig<EnemyConfig>
    {
        [NonSerialized, OdinSerialize]
        [ShowIf("IsRiflemanEnemy")]
        private RiflemanEnemyConfig _riflemanEnemyConfig;

        [NonSerialized, OdinSerialize]
        [ShowIf("IsKamikazeEnemy")]
        private KamikazeEnemyConfig _kamikazeEnemyConfig;

        [NonSerialized, OdinSerialize]
        [ShowIf("IsMachineGunnerEnemy")]
        private MachineGunnerEnemyConfig _machineGunnerEnemyConfig;
        
        public RiflemanEnemyConfig RiflemanEnemyConfig
        {
            get => _riflemanEnemyConfig;
            set => _riflemanEnemyConfig = value;
        }

        public KamikazeEnemyConfig KamikazeEnemyConfig
        {
            get => _kamikazeEnemyConfig;
            set => _kamikazeEnemyConfig = value;
        }

        public MachineGunnerEnemyConfig MachineGunnerEnemyConfig
        {
            get => _machineGunnerEnemyConfig;
            set => _machineGunnerEnemyConfig = value;
        }

        public override void SetBaseConfig(EnemyConfig baseConfig)
        {
            base.SetBaseConfig(baseConfig);

            _riflemanEnemyConfig?.SetBaseConfig(baseConfig._riflemanEnemyConfig);
            _kamikazeEnemyConfig?.SetBaseConfig(baseConfig._kamikazeEnemyConfig);
            _machineGunnerEnemyConfig?.SetBaseConfig(baseConfig._machineGunnerEnemyConfig);
        }

#if UNITY_EDITOR
        public bool IsRiflemanEnemy(Object root) =>
            IsEnemyTypeInLevelConfig(root, InteractiveObjectId.RIFLEMAN_ENEMY) ||
            IsEnemyTypeInInteractiveObjectConfig(root, InteractiveObjectId.RIFLEMAN_ENEMY);

        public bool IsKamikazeEnemy(Object root) =>
            IsEnemyTypeInLevelConfig(root, InteractiveObjectId.KAMIKAZE_ENEMY) ||
            IsEnemyTypeInInteractiveObjectConfig(root, InteractiveObjectId.KAMIKAZE_ENEMY);

        public bool IsMachineGunnerEnemy(Object root) =>
            IsEnemyTypeInLevelConfig(root, InteractiveObjectId.MACHINE_GUNNER_ENEMY) ||
            IsEnemyTypeInInteractiveObjectConfig(root, InteractiveObjectId.MACHINE_GUNNER_ENEMY);

        private bool IsEnemyTypeInLevelConfig(Object root, InteractiveObjectId enemyType)
        {
            if (root is not LevelConfig levelConfig)
                return false;

            LevelObjectData levelObject = levelConfig.LevelObjects.Find(obj => obj.CustomData.EnemyConfig == this);
            return levelObject?.ObjectType == enemyType;
        }

        private static bool IsEnemyTypeInInteractiveObjectConfig(Object root, InteractiveObjectId enemyType)
        {
            return root is InteractiveObjectConfig interactiveObjectConfig &&
                   interactiveObjectConfig.InteractiveObjectData.Id == enemyType;
        }
#endif
    }
}
