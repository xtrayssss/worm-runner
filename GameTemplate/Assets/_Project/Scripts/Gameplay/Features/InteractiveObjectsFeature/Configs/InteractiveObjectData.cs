using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StealthFeature.Configs;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record InteractiveObjectData
    {
        [Serializable]
        public class ContactDamageData
        {
            [SerializeField]
            public float Damage = -10f;

            [SerializeField]
            public StatOperation Operation;
        }

        [FormerlySerializedAs("Id")]
        [FoldoutGroup("Basic Settings", expanded: true)]
        [SerializeField] private InteractiveObjectId _id;

        [FoldoutGroup("Basic Settings")]
        [SerializeField, Required, AssetsOnly]
        [ShowIf("IsStandaloneConfig")]
        private InteractiveObjectView _interactiveObjectPrefab;

        [FoldoutGroup("Basic Settings")]
        [SerializeField]
        private float _maxHealth = 10;

        [FoldoutGroup("Reward Settings")]
        [SerializeReference]
        private Rewards _rewards;

        [FoldoutGroup("Evolutionary Incubator Settings")]
        [ShowIf("IsEvolutionaryIncubator")]
        [NonSerialized, OdinSerialize]
        private EvolutionaryIncubatorConfig _evolutionaryIncubatorConfig;

        [FoldoutGroup("Gate Settings")]
        [ShowIf("IsGate")]
        [NonSerialized, OdinSerialize]
        private GateConfig _gateConfig;

        [FoldoutGroup("Bubble Settings")]
        [ShowIf("IsBubble")]
        [NonSerialized, OdinSerialize]
        private BubbleConfig _bubbleConfig;

        [FoldoutGroup("Cannon Settings")]
        [ShowIf("IsCannon")]
        [NonSerialized, OdinSerialize]
        private CannonConfig _cannonConfig;

        [FoldoutGroup("Mine Settings")]
        [ShowIf("IsMine")]
        [NonSerialized, OdinSerialize]
        private MineConfig _mineConfig;

        [FormerlySerializedAs("_contactDamage")]
        [FoldoutGroup("Basic Settings")]
        [SerializeField]
        private ContactDamageData _contactDamageData;

        [FoldoutGroup("Enemy Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsEnemy")]
        private EnemyConfig _enemyConfig;

        [FoldoutGroup("Enemy Emplacement Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsEnemyEmplacement")]
        private EnemyEmplacementConfig _enemyEmplacementConfig;

        [FoldoutGroup("Enemy Group Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsEnemyGroup")]
        private EnemyGroupConfig _enemyGroupConfig;

        [FoldoutGroup("Capture Zone Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsCaptureZone")]
        private CaptureZoneConfig _captureZoneConfig;

        [FoldoutGroup("Collectible Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsCollectible")]
        private CollectibleConfig _collectibleConfig;

        [FoldoutGroup("Patrol Worm Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsPatrolGuard")]
        private PatrolGuardConfig _patrolGuardConfig;

        [FoldoutGroup("Watch Tower Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsWatchTower")]
        private WatchTowerConfig _watchTowerConfig;

        [FoldoutGroup("Explosive Barrel Settings")]
        [NonSerialized, OdinSerialize]
        [ShowIf("IsExplosiveBarrel")]
        private ExplosiveBarrelConfig _explosiveBarrelConfig;

        private InteractiveObjectConfig _baseConfig;

        public EvolutionaryIncubatorConfig EvolutionaryIncubatorConfig
        {
            get => _evolutionaryIncubatorConfig;
            set => _evolutionaryIncubatorConfig = value;
        }

        public ExplosiveBarrelConfig ExplosiveBarrelConfig
        {
            get => _explosiveBarrelConfig;
            set => _explosiveBarrelConfig = value;
        }

        public GateConfig GateConfig
        {
            get => _gateConfig;
            set => _gateConfig = value;
        }

        public BubbleConfig BubbleConfig
        {
            get => _bubbleConfig;
            set => _bubbleConfig = value;
        }

        public InteractiveObjectId Id
        {
            get => _id;
            set => _id = value;
        }

        public InteractiveObjectView InteractiveObjectPrefab =>
            _baseConfig != null
                ? _baseConfig.InteractiveObjectData._interactiveObjectPrefab
                : _interactiveObjectPrefab;

        public float MaxHealth
        {
            get
            {
                return _id switch
                {
                    InteractiveObjectId.ENEMY_EMPLACEMENT or
                        InteractiveObjectId.CANNON or
                        InteractiveObjectId.RIFLEMAN_ENEMY or
                        InteractiveObjectId.MACHINE_GUNNER_ENEMY or
                        InteractiveObjectId.KAMIKAZE_ENEMY or
                        InteractiveObjectId.BUBBLE or
                        InteractiveObjectId.EXPLOSIVE_BARREL or
                        InteractiveObjectId.DAMAGED_TANK
                        => _baseConfig.InteractiveObjectData._maxHealth,
                    _ => _maxHealth
                };
            }
            set => _maxHealth = value;
        }

        public Rewards Rewards
        {
            get
            {
                return _id switch
                {
                    InteractiveObjectId.CASH_STONE => _rewards,

                    _ => _baseConfig.InteractiveObjectData._rewards
                };
            }
            set => _rewards = value;
        }

        public CannonConfig CannonConfig
        {
            get => _cannonConfig;
            set => _cannonConfig = value;
        }

        public MineConfig MineConfig
        {
            get => _mineConfig;
            set => _mineConfig = value;
        }

        public ContactDamageData ContactDamage =>
            _baseConfig != null ? _baseConfig.InteractiveObjectData._contactDamageData : _contactDamageData;

        public EnemyConfig EnemyConfig
        {
            get => _enemyConfig;
            set => _enemyConfig = value;
        }

        public EnemyEmplacementConfig EnemyEmplacementConfig
        {
            get => _enemyEmplacementConfig;
            set => _enemyEmplacementConfig = value;
        }

        public EnemyGroupConfig EnemyGroupConfig
        {
            get => _enemyGroupConfig;
            set => _enemyGroupConfig = value;
        }

        public CaptureZoneConfig CaptureZoneConfig
        {
            get => _captureZoneConfig;
            set => _captureZoneConfig = value;
        }

        public CollectibleConfig CollectibleConfig
        {
            get => _collectibleConfig;
            set => _collectibleConfig = value;
        }

        public PatrolGuardConfig PatrolGuardConfig
        {
            get => _patrolGuardConfig;
            set => _patrolGuardConfig = value;
        }

        public WatchTowerConfig WatchTowerConfig
        {
            get => _watchTowerConfig;
            set => _watchTowerConfig = value;
        }

        public string Title =>
            _id switch
            {
                InteractiveObjectId.COLLECTIBLE => _id + " - " + _collectibleConfig.Type,
                _ => _id.ToString()
            };

        public bool IsEnemy() =>
            _id == InteractiveObjectId.RIFLEMAN_ENEMY ||
            _id == InteractiveObjectId.KAMIKAZE_ENEMY ||
            _id == InteractiveObjectId.MACHINE_GUNNER_ENEMY;

#if UNITY_EDITOR
        private bool IsGate() =>
            _id == InteractiveObjectId.GATE;

        private bool IsBubble() =>
            _id == InteractiveObjectId.BUBBLE;

        private bool IsEvolutionaryIncubator() =>
            _id == InteractiveObjectId.EVOLUTIONARY_INCUBATOR;

        private bool IsMine() =>
            _id == InteractiveObjectId.MINE;

        private bool IsCannon() =>
            _id == InteractiveObjectId.CANNON;

        private bool IsEnemyEmplacement() =>
            _id == InteractiveObjectId.ENEMY_EMPLACEMENT;

        private bool IsEnemyGroup() =>
            _id == InteractiveObjectId.ENEMY_GROUP;

        private bool IsCaptureZone() =>
            _id == InteractiveObjectId.CAPTURE_ZONE;

        private bool IsCollectible() =>
            _id == InteractiveObjectId.COLLECTIBLE;

        private bool IsStandaloneConfig(Object root) =>
            root is not LevelEditor and not LevelConfig;

        private bool IsPatrolGuard() =>
            _id == InteractiveObjectId.PATROL_GUARD;

        private bool IsWatchTower() =>
            _id == InteractiveObjectId.WATCH_TOWER;

        private bool IsExplosiveBarrel() =>
            _id == InteractiveObjectId.EXPLOSIVE_BARREL;
#endif
        public void InitializeFromBase(InteractiveObjectConfig baseConfig)
        {
            _baseConfig = baseConfig;

            InteractiveObjectData baseData = baseConfig.InteractiveObjectData;

            _gateConfig?.SetBaseConfig(baseData._gateConfig);
            _evolutionaryIncubatorConfig?.SetBaseConfig(baseData._evolutionaryIncubatorConfig);
            _mineConfig?.SetBaseConfig(baseData._mineConfig);
            _cannonConfig?.SetBaseConfig(baseData._cannonConfig);
            _enemyConfig?.SetBaseConfig(baseData._enemyConfig);
            _bubbleConfig?.SetBaseConfig(baseData._bubbleConfig);
            _watchTowerConfig?.SetBaseConfig(baseData._watchTowerConfig);
            _patrolGuardConfig?.SetBaseConfig(baseData._patrolGuardConfig);
            _collectibleConfig?.SetBaseConfig(baseData._collectibleConfig);
            _explosiveBarrelConfig?.SetBaseConfig(baseData._explosiveBarrelConfig);
            _enemyEmplacementConfig?.SetBaseConfig(baseData._enemyEmplacementConfig);
        }
    }
}