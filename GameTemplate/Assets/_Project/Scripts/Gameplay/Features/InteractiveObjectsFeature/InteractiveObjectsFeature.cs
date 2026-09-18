using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature
{
    public sealed class InteractiveObjectsFeature : IFeature
    {
        private readonly AllServices _services;

        public InteractiveObjectsFeature(AllServices services) =>
            _services = services;

        public void Configure(FeatureContext context)
        {
            RewardService rewardService = _services.Get<RewardService>();
            TargetingService targetingService = _services.Get<TargetingService>();
            ProjectileFactory projectileFactory = _services.Get<ProjectileFactory>();
            RunStatisticsService runStatisticsService = _services.Get<RunStatisticsService>();
            VFXService vfxService = _services.Get<VFXService>();
            StatsService statsService = _services.Get<StatsService>();
            EnhancementService enhancementService = _services.Get<EnhancementService>();
            LevelService levelService = _services.Get<LevelService>();
            MortarStrikeFactory mortarStrikeFactory = _services.Get<MortarStrikeFactory>();
            SoulFactory soulFactory = _services.Get<SoulFactory>();
            CrowdFactory crowdFactory = _services.Get<CrowdFactory>();
            InteractiveObjectFactory interactiveObjectFactory = _services.Get<InteractiveObjectFactory>();
            GameStateMachine gameStateMachine = _services.Get<GameStateMachine>();
            CollectibleService collectibleService = _services.Get<CollectibleService>();
            CaptureZoneService captureZoneService = _services.Get<CaptureZoneService>();
            ConfigsService configsService = _services.Get<ConfigsService>();
            UIRoot uiRoot = _services.Get<UIRoot>();
            AudioService audioService = _services.Get<AudioService>();

            context
                // common
                .AddSystem(new InteractiveObjectCrowdCollisionSystem(statsService))

                // cash stone
                .AddSystem(new CashStoneDestructionSystem(
                    rewardService,
                    enhancementService,
                    runStatisticsService))

                // barrel
                .AddSystem(new BarrelDestructionSystem(
                    rewardService,
                    enhancementService,
                    runStatisticsService))

                // damaged tank
                .AddSystem(new DamagedTankDestructionSystem(
                    rewardService,
                    enhancementService,
                    runStatisticsService))

                // explosive barrel
                .AddSystem(new ExplosiveBarrelExplosionSystem(statsService))
                .AddSystem(new ExplosiveBarrelDestructionSystem())

                // cannon
                .AddSystem(new CannonFiringSystem(projectileFactory, gameStateMachine))
                .AddSystem(new CannonMovementSystem())
                .AddSystem(new CannonDestructionSystem(
                    rewardService,
                    enhancementService,
                    runStatisticsService))

                // gate
                .AddSystem(new GateActivationSystem(enhancementService))
                .AddSystem(new GateEnhancementSystem())
                .AddSystem(new GateDestructionSystem())
                .AddSystem(new GateVisualSystem(enhancementService))
                .AddSystem(new GateMovementSystem(gameStateMachine, levelService))

                // evolutionary incubator
                .AddSystem(new EvolutionaryIncubatorActivationSystem(configsService))
                .AddSystem(new EvolutionaryIncubatorEnhancementSystem())
                .AddSystem(new EvolutionaryIncubatorVisualSystem(vfxService, configsService))
                .AddSystem(new EvolutionaryIncubatorDestructionSystem())

                // bonus chest
                .AddSystem(new BonusChestTriggerSystem(
                    runStatisticsService,
                    rewardService, 
                    enhancementService))

                // mine
                .AddSystem(new MineExplosionSystem(targetingService))
                .AddSystem(new MineDestructionSystem())

                // mortar
                .AddSystem(new MortarStrikeSchedulerSystem(mortarStrikeFactory, levelService))
                .AddSystem(new MortarStrikeSystem(targetingService))

                // enemy
                .AddSystem(new RiflemanShootingSystem(projectileFactory, gameStateMachine))
                .AddSystem(new MachineGunnerShootingSystem(projectileFactory, gameStateMachine))
                .AddSystem(new EnemyDestructionSystem(
                    soulFactory,
                    rewardService,
                    enhancementService,
                    runStatisticsService))
                .AddSystem(new EnemySpawnSystem(levelService, interactiveObjectFactory, gameStateMachine))
                .AddSystem(new EnemyMovementSystem(levelService, gameStateMachine))
                .AddSystem(new EnemyExplosionSystem(
                    levelService,
                    statsService,
                    gameStateMachine,
                    targetingService))
                .AddSystem(new EnemyCountTrackingSystem(levelService, gameStateMachine))

                // bubble
                .AddSystem(new BubbleMovementSystem(gameStateMachine, levelService))
                .AddSystem(new BubbleActivationSystem(enhancementService))
                .AddSystem(new BubbleDestructionSystem())
                .AddSystem(new BubbleSlowdownSystem())

                // enemy emplacement
                .AddSystem(new EnemyEmplacementDestructionSystem(
                    rewardService,
                    enhancementService,
                    runStatisticsService))

                // capture zone
                .AddSystem(new CaptureZoneSystem(
                    crowdFactory,
                    captureZoneService,
                    uiRoot,
                    audioService,
                    configsService))

                // collectible
                .AddSystem(new CollectibleCollectionSystem(collectibleService, uiRoot))
                .AddSystem(new CollectibleDestructionSystem())

                //
                .AddSystem(new HitAnimationSystem())
                ;
        }
    }
}