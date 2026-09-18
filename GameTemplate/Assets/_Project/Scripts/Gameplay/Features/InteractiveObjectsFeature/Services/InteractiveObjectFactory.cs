using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.LifeForceFeature;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using _Project.Scripts.Gameplay.Features.RewardFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StealthFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StealthFeature.Components;
using _Project.Scripts.Gameplay.Features.StealthFeature.Configs;
using _Project.Scripts.Gameplay.Features.StealthFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using JetBrains.Annotations;
using Scellecs.Morpeh;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services
{
    public sealed class InteractiveObjectFactory : IService
    {
        private readonly ConfigsService _configsService;
        private readonly VFXService _vfxService;
        private readonly CrowdFactory _crowdFactory;
        private readonly GateVisualSystem _gateVisualSystem;
        private readonly CameraService _cameraService;
        private readonly AudioService _audioService;
        private readonly UIRoot _uiRoot;

        public InteractiveObjectFactory(
            ConfigsService configsService,
            VFXService vfxService,
            CrowdFactory crowdFactory,
            GateVisualSystem gateVisualSystem,
            CameraService cameraService,
            AudioService audioService,
            UIRoot uiRoot)
        {
            _configsService = configsService;
            _vfxService = vfxService;
            _crowdFactory = crowdFactory;
            _gateVisualSystem = gateVisualSystem;
            _cameraService = cameraService;
            _audioService = audioService;
            _uiRoot = uiRoot;
        }

        public Entity CreateInteractiveObject(LevelObjectData levelObjectData, Transform parent)
        {
            InteractiveObjectConfig baseConfig = _configsService.GetInteractiveObjectConfig(levelObjectData.ObjectType);
            levelObjectData.CustomData.InitializeFromBase(baseConfig);

            InteractiveObjectConfig finalConfig = ScriptableObject.CreateInstance<InteractiveObjectConfig>();
            finalConfig.InteractiveObjectData = levelObjectData.CustomData;

            Entity entity = CreateFromConfig(finalConfig, levelObjectData);

            if (entity.Has<EntityViewLink>())
            {
                ref EntityViewLink viewLink = ref entity.GetComponent<EntityViewLink>();
                viewLink.View.transform.SetParent(parent);
            }

            return entity;
        }

        private Entity CreateFromConfig(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            return config.InteractiveObjectData.Id switch
            {
                InteractiveObjectId.CASH_STONE => CreateCashStone(config, levelObjectData).entity,
                InteractiveObjectId.CANNON => CreateCannon(config, levelObjectData).entity,
                InteractiveObjectId.GATE => CreateGate(config, levelObjectData).entity,
                InteractiveObjectId.BUBBLE => CreateBubble(config, levelObjectData).entity,
                InteractiveObjectId.EVOLUTIONARY_INCUBATOR => CreateEvolutionaryIncubator(config, levelObjectData)
                    .entity,
                InteractiveObjectId.BONUS_CHEST => CreateBonusChest(config, levelObjectData).entity,
                InteractiveObjectId.MINE => CreateMine(config, levelObjectData).entity,
                InteractiveObjectId.BARREL => CreateBarrel(config, levelObjectData).entity,
                InteractiveObjectId.RIFLEMAN_ENEMY => CreateRiflemanEnemy(config, levelObjectData).entity,
                InteractiveObjectId.MACHINE_GUNNER_ENEMY => CreateMachineGunnerEnemy(config, levelObjectData).entity,
                InteractiveObjectId.KAMIKAZE_ENEMY => CreateKamikazeEnemy(config, levelObjectData).entity,
                InteractiveObjectId.FINISH_TRIGGER => CreateFinishTrigger(config, levelObjectData).entity,
                InteractiveObjectId.ENEMY_EMPLACEMENT => CreateEnemyEmplacement(config, levelObjectData).entity,
                InteractiveObjectId.CAPTURE_ZONE => CreateCaptureZone(config, levelObjectData).entity,
                InteractiveObjectId.COLLECTIBLE => CreateCollectible(config, levelObjectData).entity,
                InteractiveObjectId.PATROL_GUARD => CreatePatrolGuard(config, levelObjectData).entity,
                InteractiveObjectId.WATCH_TOWER => CreateWatchTower(config, levelObjectData).entity,
                InteractiveObjectId.DAMAGED_TANK => CreateDamagedTank(config, levelObjectData).entity,
                InteractiveObjectId.EXPLOSIVE_BARREL => CreateExplosiveBarrel(config, levelObjectData).entity,
                _ => throw new ArgumentException($"Unknown interactive object type: {config.InteractiveObjectData.Id}")
            };
        }

        private (Entity entity, TView view) CreateBaseInteractiveObject<TView>(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData,
            [CanBeNull] InteractiveObjectView customPrefab = null)
            where TView : InteractiveObjectView
        {
            World world = World.Default;
            Entity interactiveObject = world.CreateEntity();

            InteractiveObjectView prefab = customPrefab ?? config.InteractiveObjectData.InteractiveObjectPrefab;
            Vector3 position = levelObjectData.Position;

            Quaternion rotation = prefab.transform.rotation;

            InteractiveObjectView view = Object.Instantiate(prefab, position, rotation);

            view.Construct(_vfxService, originalScale: view.transform.localScale);
            view.Entity = interactiveObject;

            interactiveObject.AddComponent<InteractiveObjectTag>();

            interactiveObject.AddComponent<Position>() = new Position
            {
                Value = position
            };

            interactiveObject.AddComponent<EntityViewLink>() = new EntityViewLink
            {
                Prefab = config.InteractiveObjectData.InteractiveObjectPrefab,
                View = view
            };

            ref Stats stats = ref interactiveObject.AddComponent<Stats>();
            stats = new StatsBuilder()
                .SetStat(StatId.HEALTH, config.InteractiveObjectData.MaxHealth)
                .Build();

            interactiveObject.AddComponent<Health>() = new Health
            {
                CurrentHealth = stats.Value[StatId.HEALTH].CurrentValue,
                MaxHealth = stats.Value[StatId.HEALTH].CachedModifiedBaseValue
            };

            interactiveObject.AddComponent<ColliderLink>() = new ColliderLink
            {
                Value = view.Collider
            };

            if (config.InteractiveObjectData.ContactDamage is { Damage: <= 0 } contactDamage)
            {
                interactiveObject.AddComponent<ContactDamage>() = new ContactDamage
                {
                    Damage = contactDamage.Damage,
                    Operation = contactDamage.Operation
                };
            }

            if (view.HealthBar != null)
            {
                interactiveObject.AddComponent<HealthDisplayLink>() = new HealthDisplayLink
                {
                    Value = view.HealthBar
                };

                interactiveObject.AddComponent<HealthDisplaySettings>() = new HealthDisplaySettings
                {
                    UpdateMode = StatDisplayUpdateMode.ALWAYS
                };
            }

            if (config.InteractiveObjectData.Rewards != null)
            {
                interactiveObject.AddComponent<RewardsLink>() = new RewardsLink
                {
                    Value = config.InteractiveObjectData.Rewards
                };
            }

            interactiveObject.AddComponent<DestructionVFXLink>() = new DestructionVFXLink
            {
                Value = view.DestructionVFX
            };

            interactiveObject.AddComponent<ShowDamagePopupMarker>();
            interactiveObject.AddComponent<HitAnimationMarker>();

            return (interactiveObject, (TView)view);
        }

        private (Entity entity, MineView view) CreateMine(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            MineConfig mineConfig = config.InteractiveObjectData.MineConfig;
            (Entity entity, MineView view) mine = CreateBaseInteractiveObject<MineView>(config, levelObjectData);

            mine.view.Construct(_cameraService);

            mine.entity.AddComponent<MineTag>();

            mine.entity.AddComponent<MineState>() = new MineState
            {
                ArmingTimer = mineConfig.ArmingDelay,
                ExplosionRadius = mineConfig.ExplosionRadius,

#if UNITY_EDITOR
                ShowDetectionRadius = mineConfig.ShowDetectionRadius
#endif
            };

            mine.view.CreateRangeIndicator(mineConfig.ExplosionRadius);

#if DEBUG
            mine.entity.AddComponent<EntityName>().Value = $"Mine_{mine.entity.Id}";
#endif

            return mine;
        }

        private (Entity entity, InteractiveObjectView view) CreateBonusChest(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, BonusChestView view) chest =
                CreateBaseInteractiveObject<BonusChestView>(config, levelObjectData);

            chest.view.Construct(_vfxService, _audioService);
            chest.entity.AddComponent<BonusChestTag>();

#if DEBUG
            chest.entity.AddComponent<EntityName>().Value = $"BonusChest_{chest.entity.Id}";
#endif

            chest.view.StartShake();

            return chest;
        }

        private (Entity entity, GateView view) CreateGate(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, GateView view) gate = CreateBaseInteractiveObject<GateView>(config, levelObjectData);

            GateConfig gateConfig = config.InteractiveObjectData.GateConfig;

            gate.view.Construct(
                effectType: gateConfig.EffectType,
                targetStat: gateConfig.StatConfig.TargetStat);

            gate.entity.AddComponent<GateTag>();
            gate.entity.AddComponent<ImmortalMarker>();

            gate.entity.AddComponent<GateState>() = new GateState
            {
                Step = gateConfig.Step,
                EffectValue = gateConfig.EffectValue,
                EffectType = gateConfig.EffectType
            };

            if (gateConfig.IsEnhanceable)
                gate.entity.AddComponent<IsEnhanceableMarker>();

            LevelService levelService = AllServices.Instance.Get<LevelService>();

            if (levelService.CurrentLevel.LevelMode == LevelMode.DEFENSE)
            {
                const float MOVE_SPEED = 3f;

                gate.entity.AddComponent<GateMovement>() = new GateMovement
                {
                    MoveSpeed = MOVE_SPEED
                };
            }

            AddEffect(
                source: gate.entity,
                gateConfig.EffectType,
                gateConfig.StatConfig.TargetStat,
                gateConfig.EffectValue);

            _gateVisualSystem.UpdateGateSprite(gate.entity, gate.view);

#if DEBUG
            gate.entity.AddComponent<EntityName>().Value = $"Gate_{gate.entity.Id}";
#endif

            return gate;
        }

        private (Entity entity, BubbleView view) CreateBubble(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, BubbleView view) bubble = CreateBaseInteractiveObject<BubbleView>(config, levelObjectData);

            BubbleConfig bubbleConfig = config.InteractiveObjectData.BubbleConfig;

            bubble.view.Construct(
                _configsService,
                effectValue: bubbleConfig.EffectValue,
                effectType: bubbleConfig.EffectType,
                targetStat: bubbleConfig.StatConfig.TargetStat
            );

            bubble.entity.AddComponent<BubbleTag>();

            const float MOVE_SPEED = 3;

            bubble.entity.AddComponent<BubbleMovement>() = new BubbleMovement
            {
                CurrentMoveSpeed = MOVE_SPEED,
                OriginalMoveSpeed = MOVE_SPEED
            };

            bubble.entity.AddComponent<BubbleState>() = new BubbleState
            {
                EffectValue = bubbleConfig.EffectValue,
                EffectType = bubbleConfig.EffectType,
                Step = 0.005f
            };

            AddEffect(
                source: bubble.entity,
                bubbleConfig.EffectType,
                bubbleConfig.StatConfig.TargetStat,
                bubbleConfig.EffectValue);

            bubble.view.PlayFloatingAnimation();

#if DEBUG
            bubble.entity.AddComponent<EntityName>().Value = $"Bubble_{bubble.entity.Id}";
#endif

            return bubble;
        }

        private (Entity entity, EvolutionaryIncubatorView view) CreateEvolutionaryIncubator(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, EvolutionaryIncubatorView view) incubator =
                CreateBaseInteractiveObject<EvolutionaryIncubatorView>(config, levelObjectData);

            incubator.view.Construct(_configsService, _crowdFactory);

            incubator.entity.AddComponent<EvolutionaryIncubatorTag>();
            incubator.entity.AddComponent<ImmortalMarker>();

            ref EvolutionaryIncubatorState incubatorState =
                ref incubator.entity.AddComponent<EvolutionaryIncubatorState>();

            incubatorState = new EvolutionaryIncubatorState
            {
                TierLevel = 1,
                SpawnCount = config.InteractiveObjectData.EvolutionaryIncubatorConfig.SpawnCount,
                AccumulatedDamage = 0f,
                DamageThresholdsPerLevel =
                    config.InteractiveObjectData.EvolutionaryIncubatorConfig.DamageThresholdsPerLevel
            };

            if (config.InteractiveObjectData.EvolutionaryIncubatorConfig.IsEnhanceable)
                incubator.entity.AddComponent<IsEnhanceableMarker>();

            incubator.entity.AddComponent<EvolutionDisplaySettings>() = new EvolutionDisplaySettings
            {
                Display = incubator.view.EvolutionProgressDisplay,
                UpdateMode = StatDisplayUpdateMode.ALWAYS
            };

            incubator.view.EvolutionProgressDisplay.UpdateLevel(incubatorState.TierLevel);
            incubator.view.UpdateMemberDisplay(incubatorState.SpawnCount, incubatorState.TierLevel);

#if DEBUG
            incubator.entity.AddComponent<EntityName>().Value = $"EvolutionaryIncubator_{incubator.entity.Id}";
#endif

            return incubator;
        }

        private (Entity entity, CashStoneView view) CreateCashStone(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, CashStoneView view) cashStone =
                CreateBaseInteractiveObject<CashStoneView>(config, levelObjectData);

            cashStone.entity.AddComponent<CashStoneTag>();

#if DEBUG
            cashStone.entity.AddComponent<EntityName>().Value = $"CashStone_{cashStone.entity.Id}";
#endif

            return cashStone;
        }

        private (Entity entity, BarrelView view) CreateBarrel(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, BarrelView view) barrel = CreateBaseInteractiveObject<BarrelView>(config, levelObjectData);

            barrel.entity.AddComponent<BarrelTag>();

#if DEBUG
            barrel.entity.AddComponent<EntityName>().Value = $"Barrel_{barrel.entity.Id}";
#endif

            return barrel;
        }

        private (Entity entity, ExplosiveBarrelView view) CreateExplosiveBarrel(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, ExplosiveBarrelView view) barrel =
                CreateBaseInteractiveObject<ExplosiveBarrelView>(config, levelObjectData);

            barrel.view.Construct(_cameraService);
            barrel.entity.AddComponent<ExplosiveBarrelTag>();

            ExplosiveBarrelConfig barrelConfig = config.InteractiveObjectData.ExplosiveBarrelConfig;

            barrel.entity.AddComponent<ExplosiveBarrel>() = new ExplosiveBarrel
            {
                ExplosionRadius = barrelConfig.ExplosionRadius,
                ExplosionDamage = barrelConfig.ExplosionDamage
            };

#if DEBUG
            barrel.entity.AddComponent<EntityName>().Value = $"ExplosiveBarrel_{barrel.entity.Id}";
#endif

            return barrel;
        }

        private (Entity entity, CannonView view) CreateCannon(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, CannonView view) cannon = CreateBaseInteractiveObject<CannonView>(config, levelObjectData);

            cannon.view.Construct(_vfxService);

            cannon.entity.AddComponent<CannonTag>();

            cannon.entity.AddComponent<CannonWeapon>() = new CannonWeapon
            {
                DetectionRange = config.InteractiveObjectData.CannonConfig.DetectionRange,
                ShootPoint = cannon.view.ShootPoint
            };

            ref Stats stats = ref cannon.entity.GetComponent<Stats>();
            stats
                .WithDamage(config.InteractiveObjectData.CannonConfig.ProjectileDamage)
                .WithAttackCooldown(config.InteractiveObjectData.CannonConfig.ShootingCooldown);

            cannon.entity.AddComponent<Damage>() = new Damage
            {
                CurrentDamage = stats.Value[StatId.DAMAGE].CurrentValue
            };

            cannon.entity.AddComponent<AttackCooldown>() = new AttackCooldown
            {
                Cooldown = stats.Value[StatId.COOLDOWN].CurrentValue
            };

            cannon.entity.AddComponent<StatModifierSetups>().Value = new StatModifierSetup[]
            {
                new HealthModifierSetup
                {
                    TargetStatId = StatId.HEALTH,
                    Operation = StatOperation.ADD,
                    Priority = (int)StatOperation.ADD,
                    EffectType = StatEffectType.INSTANT,
                    AffectsBaseValue = false,
                    SourceStatId = StatId.DAMAGE
                }
            };

#if DEBUG
            cannon.entity.AddComponent<EntityName>().Value = $"Cannon_{cannon.entity.Id}";
#endif

            return cannon;
        }

        private void AddEffect(Entity source, EffectType effectType, StatId targetStat, float baseEffectValue)
        {
            switch (effectType)
            {
                case EffectType.STAT_MODIFICATION:
                    source.AddComponent<StatModificationEffect>() = new StatModificationEffect
                    {
                        TargetStat = targetStat
                    };
                    break;

                case EffectType.POPULATION_ADD:
                case EffectType.POPULATION_MULTIPLY:
                case EffectType.POPULATION_SUBTRACT:
                    source.AddComponent<PopulationModificationEffect>() = new PopulationModificationEffect
                    {
                        Value = (int)baseEffectValue
                    };
                    break;

                case EffectType.EVOLUTION_UPGRADE:
                    source.AddComponent<EvolutionEffect>() = new EvolutionEffect
                    {
                        EvolutionLevelChange = 1
                    };
                    break;

                case EffectType.EVOLUTION_DOWNGRADE:
                    source.AddComponent<EvolutionEffect>() = new EvolutionEffect
                    {
                        EvolutionLevelChange = -1
                    };
                    break;
            }
        }

        private (Entity entity, FinishTriggerView view) CreateFinishTrigger(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, FinishTriggerView view) trigger =
                CreateBaseInteractiveObject<FinishTriggerView>(config, levelObjectData);

            trigger.view.Construct(_vfxService, _uiRoot);

            trigger.entity.AddComponent<FinishTrigger>();

#if DEBUG
            trigger.entity.AddComponent<EntityName>().Value = $"FinishTrigger_{trigger.entity.Id}";
#endif

            return trigger;
        }

        private (Entity entity, EnemyEmplacementView view) CreateEnemyEmplacement(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, EnemyEmplacementView view) emplacement =
                CreateBaseInteractiveObject<EnemyEmplacementView>(config, levelObjectData);

            emplacement.entity.AddComponent<EnemyEmplacementTag>();

            ref EnemyEmplacementState state = ref emplacement.entity.AddComponent<EnemyEmplacementState>();

            state = new EnemyEmplacementState
            {
                Enemies = new Entity[emplacement.view.SpawnPoints.Length]
            };

            for (int index = 0; index < emplacement.view.SpawnPoints.Length; index++)
            {
                Transform spawnPoint = emplacement.view.SpawnPoints[index];

                LevelObjectData enemyLevelObject = ObjectGenerator.GenerateObject<LevelObjectData>(
                    id: InteractiveObjectId.RIFLEMAN_ENEMY,
                    position: spawnPoint.position,
                    customData: new InteractiveObjectData
                    {
                        EnemyConfig = new EnemyConfig
                        {
                            RiflemanEnemyConfig = new RiflemanEnemyConfig()
                        }
                    }
                );

                Entity enemy = CreateInteractiveObject(
                    enemyLevelObject,
                    spawnPoint.transform);

                ref ColliderLink colliderLink = ref enemy.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                enemy.AddComponent<ImmortalMarker>();

                enemy.RemoveComponent<HitAnimationMarker>();

                ref EntityViewLink enemyViewLink = ref enemy.GetComponent<EntityViewLink>();
                EnemyView enemyView = (EnemyView)enemyViewLink.View;

                if (enemyView.HealthBar != null)
                {
                    enemy.RemoveComponent<HealthDisplayLink>();
                    enemy.RemoveComponent<HealthDisplaySettings>();

                    enemyView.HealthBar.gameObject.SetActive(false);
                }

                enemyView.StartIdle();

                state.Enemies[index] = enemy;
            }

#if DEBUG
            emplacement.entity.AddComponent<EntityName>().Value = $"EnemyEmplacement_{emplacement.entity.Id}";
#endif

            return emplacement;
        }

        public Entity[] CreateEnemyGroup(LevelObjectData levelObjectData, Transform parent)
        {
            EnemyGroup enemyGroup = (EnemyGroup)levelObjectData;

            Entity[] enemies = new Entity[enemyGroup.Enemies.Count];

            for (int i = 0; i < enemyGroup.Enemies.Count; i++)
            {
                LevelObjectData enemyData = enemyGroup.Enemies[i];

                Entity enemy = CreateInteractiveObject(
                    enemyData,
                    parent);

                ref EntityViewLink enemyViewLink = ref enemy.GetComponent<EntityViewLink>();
                EnemyView enemyView = (EnemyView)enemyViewLink.View;

                enemyView.StartIdle();

                enemies[i] = enemy;
            }

            return enemies;
        }

        private (Entity entity, CaptureZoneView view) CreateCaptureZone(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            CaptureZoneConfig captureZoneConfig = config.InteractiveObjectData.CaptureZoneConfig;

            (Entity entity, CaptureZoneView view) zone =
                CreateBaseInteractiveObject<CaptureZoneView>(config, levelObjectData);

            zone.entity.AddComponent<CaptureZoneTag>();
            zone.entity.AddComponent<CaptureZoneState>() = new CaptureZoneState
            {
                CapturedMembers = new List<Entity>(),
                RequiredMembers = captureZoneConfig.RequiredMembers
            };

#if DEBUG
            zone.entity.AddComponent<EntityName>().Value = $"CaptureZone_{zone.entity.Id}";
#endif

            return zone;
        }

        private (Entity entity, CollectibleView view) CreateCollectible(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            CollectibleConfig collectibleConfig = config.InteractiveObjectData.CollectibleConfig;

            (Entity entity, CollectibleView view) collectible =
                CreateBaseInteractiveObject<CollectibleView>(
                    config,
                    levelObjectData,
                    customPrefab: collectibleConfig.CollectibleByType[collectibleConfig.Type]);

            collectible.entity.AddComponent<CollectibleTag>();

            collectible.entity.AddComponent<CollectibleTypeLink>() = new CollectibleTypeLink
            {
                Value = collectibleConfig.Type
            };

            collectible.view.PlayFloatingAnimation();

#if DEBUG
            collectible.entity.AddComponent<EntityName>().Value = $"Collectible_{collectible.entity.Id}";
#endif

            return collectible;
        }

        private (Entity entity, PatrolGuardView view) CreatePatrolGuard(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, PatrolGuardView view) guard =
                CreateBaseInteractiveObject<PatrolGuardView>(config, levelObjectData);

            guard.entity.AddComponent<PatrolGuardTag>();

            PatrolGuardConfig guardConfig = config.InteractiveObjectData.PatrolGuardConfig;

            guard.entity.AddComponent<VisionCone>() = new VisionCone
            {
                VisionRange = guardConfig.StealthObjectConfig.VisionRange,
                VisionAngle = guardConfig.StealthObjectConfig.VisionAngle,
            };

            guard.entity.AddComponent<PatrolGuard>() = new PatrolGuard
            {
                RotationSpeed = guardConfig.RotationSpeed,
                MaxRotationAngle = guardConfig.MaxRotationAngle,
                CurrentRotation = 0f,
                RotationDirection = 1
            };

            guard.view.UpdateVisionCone(
                guardConfig.StealthObjectConfig.VisionRange,
                guardConfig.StealthObjectConfig.VisionAngle,
                ConeMeshGenerator.ConeDirection.BACKWARD);

            guard.view.StartIdle();

#if DEBUG
            guard.entity.AddComponent<EntityName>().Value = $"PatrolGuard_{guard.entity.Id}";
#endif

            return guard;
        }

        private (Entity entity, DamagedTankView view) CreateDamagedTank(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, DamagedTankView view) tank =
                CreateBaseInteractiveObject<DamagedTankView>(config, levelObjectData);

            tank.entity.AddComponent<DamagedTankTag>();

#if DEBUG
            tank.entity.AddComponent<EntityName>().Value = $"DamagedTank_{tank.entity.Id}";
#endif

            return tank;
        }

        private (Entity entity, WatchTowerView view) CreateWatchTower(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, WatchTowerView view) tower =
                CreateBaseInteractiveObject<WatchTowerView>(config, levelObjectData);

            float xPosition = levelObjectData.Position.x;
            bool isOnRightSide = xPosition > 0f;

            if (isOnRightSide)
            {
                Quaternion towerRotation = tower.view.transform.localRotation;

                tower.view.transform.localRotation =
                    Quaternion.Euler(towerRotation.eulerAngles.x, 180f, towerRotation.eulerAngles.z);
            }

            tower.entity.AddComponent<WatchTowerTag>();

            WatchTowerConfig towerConfig = config.InteractiveObjectData.WatchTowerConfig;

            tower.entity.AddComponent<VisionCone>() = new VisionCone
            {
                VisionRange = towerConfig.StealthObjectConfig.VisionRange,
                VisionAngle = towerConfig.StealthObjectConfig.VisionAngle,
            };

            tower.entity.AddComponent<WatchTower>() = new WatchTower
            {
                SweepDuration = towerConfig.SweepDuration
            };

            tower.view.UpdateVisionCone(
                towerConfig.StealthObjectConfig.VisionRange,
                towerConfig.StealthObjectConfig.VisionAngle);

#if DEBUG
            tower.entity.AddComponent<EntityName>().Value = $"WatchTower_{tower.entity.Id}";
#endif

            return tower;
        }

        private (Entity entity, EnemyView view) CreateRiflemanEnemy(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, EnemyView view) enemy = CreateBaseEnemy(config, levelObjectData);

            RiflemanEnemyConfig enemyConfig = config.InteractiveObjectData.EnemyConfig.RiflemanEnemyConfig;

            const float STOP_DISTANCE = 10f;
            const float SHOOT_COOLDOWN = 2f;
            const float MOVE_SPEED = 3f;

            enemy.entity.AddComponent<RiflemanEnemyTag>();

            enemy.entity.AddComponent<ShooterEnemy>() = new ShooterEnemy
            {
                StopDistance = STOP_DISTANCE
            };

            enemy.entity.AddComponent<EnemyMovement>() = new EnemyMovement
            {
                MoveSpeed = MOVE_SPEED
            };

            ref Stats stats = ref enemy.entity.GetComponent<Stats>();

            stats
                .WithDamage(enemyConfig.ShootDamage)
                .WithAttackCooldown(SHOOT_COOLDOWN);

            enemy.entity.AddComponent<Damage>() = new Damage
            {
                CurrentDamage = stats.Value[StatId.DAMAGE].CurrentValue
            };

            enemy.entity.AddComponent<AttackCooldown>() = new AttackCooldown
            {
                Cooldown = stats.Value[StatId.COOLDOWN].CurrentValue
            };

#if DEBUG
            enemy.entity.AddComponent<EntityName>().Value = $"RiflemanEnemy_{enemy.entity.Id}";
#endif

            return enemy;
        }

        private (Entity entity, EnemyView view) CreateMachineGunnerEnemy(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, EnemyView view) enemy = CreateBaseEnemy(config, levelObjectData);

            MachineGunnerEnemyConfig enemyConfig = config.InteractiveObjectData.EnemyConfig.MachineGunnerEnemyConfig;

            const float STOP_DISTANCE = 10f;
            const float MOVE_SPEED = 2f;

            enemy.entity.AddComponent<MachineGunnerEnemy>() = new MachineGunnerEnemy
            {
                BurstCount = enemyConfig.BurstCount,
                BurstInterval = enemyConfig.BurstInterval,
                BurstCooldown = enemyConfig.BurstCooldown
            };

            enemy.entity.AddComponent<ShooterEnemy>() = new ShooterEnemy
            {
                StopDistance = STOP_DISTANCE
            };

            enemy.entity.AddComponent<MachineGunnerBurstState>();

            enemy.entity.AddComponent<EnemyMovement>() = new EnemyMovement
            {
                MoveSpeed = MOVE_SPEED
            };

            ref Stats stats = ref enemy.entity.GetComponent<Stats>();
            stats
                .WithDamage(enemyConfig.ShootDamage)
                .WithAttackCooldown(enemyConfig.BurstCooldown);

            enemy.entity.AddComponent<Damage>() = new Damage
            {
                CurrentDamage = stats.Value[StatId.DAMAGE].CurrentValue
            };

            enemy.entity.AddComponent<AttackCooldown>() = new AttackCooldown
            {
                Cooldown = stats.Value[StatId.COOLDOWN].CurrentValue
            };

#if DEBUG
            enemy.entity.AddComponent<EntityName>().Value = $"MachineGunnerEnemy_{enemy.entity.Id}";
#endif

            return enemy;
        }

        private (Entity entity, EnemyView view) CreateKamikazeEnemy(InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, EnemyView view) enemy = CreateBaseEnemy(config, levelObjectData);

            const float MOVE_SPEED = 4f;

            KamikazeEnemyConfig kamikazeEnemyConfig = config.InteractiveObjectData.EnemyConfig.KamikazeEnemyConfig;

            float explosionDamage = kamikazeEnemyConfig.ExplosionDamage;

            Vector3 explosionOffset = Vector3.forward * 1f;

            enemy.entity.AddComponent<KamikazeEnemy>() = new KamikazeEnemy
            {
                ExplosionRadius = kamikazeEnemyConfig.ExplosionRadius,
                ExplosionDamage = explosionDamage,
                ExplosionOffset = explosionOffset
            };

            enemy.entity.AddComponent<EnemyMovement>() = new EnemyMovement
            {
                MoveSpeed = MOVE_SPEED
            };

            ref Stats stats = ref enemy.entity.GetComponent<Stats>();

            stats
                .WithDamage(explosionDamage);

            enemy.entity.AddComponent<Damage>() = new Damage
            {
                CurrentDamage = stats.Value[StatId.DAMAGE].CurrentValue
            };

#if DEBUG
            enemy.entity.AddComponent<EntityName>().Value = $"KamikazeEnemy_{enemy.entity.Id}";
#endif

            return enemy;
        }

        private (Entity entity, EnemyView view) CreateBaseEnemy(
            InteractiveObjectConfig config,
            LevelObjectData levelObjectData)
        {
            (Entity entity, EnemyView view) enemy = CreateBaseInteractiveObject<EnemyView>(config, levelObjectData);

            enemy.view.Construct(_vfxService, _cameraService);

            enemy.entity.AddComponent<EnemyTag>();

            enemy.entity.AddComponent<StatModifierSetups>().Value = new StatModifierSetup[]
            {
                new HealthModifierSetup
                {
                    TargetStatId = StatId.HEALTH,
                    Operation = StatOperation.ADD,
                    Priority = (int)StatOperation.ADD,
                    EffectType = StatEffectType.INSTANT,
                    AffectsBaseValue = false,
                    SourceStatId = StatId.DAMAGE
                }
            };

            LevelService levelService = AllServices.Instance.Get<LevelService>();

            if (levelService.CurrentLevel.LevelMode == LevelMode.DEFENSE)
            {
                if (enemy.view.HealthBar != null)
                {
                    enemy.entity.RemoveComponent<HealthDisplayLink>();
                    enemy.entity.RemoveComponent<HealthDisplaySettings>();

                    enemy.view.HealthBar.gameObject.SetActive(false);
                }
            }

            return enemy;
        }
    }
}