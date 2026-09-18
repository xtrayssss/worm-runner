using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Systems;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.InputFeature.Systems;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature
{
    public sealed class CrowdFeature : IFeature
    {
        private readonly AllServices _services;

        public CrowdFeature(AllServices services) =>
            _services = services;

        public void Configure(FeatureContext context)
        {
            CrowdFactory crowdFactory = _services.Get<CrowdFactory>();
            InputService inputService = _services.Get<InputService>();
            ProjectileFactory projectileFactory = _services.Get<ProjectileFactory>();
            GameStateMachine gameStateMachine = _services.Get<GameStateMachine>();
            CrowdEvolutionSystem crowdEvolutionSystem = _services.Get<CrowdEvolutionSystem>();
            LevelService levelService = _services.Get<LevelService>();
            CrowdTriangleFormationService crowdTriangleFormationService =
                _services.Get<CrowdTriangleFormationService>();
            CameraService cameraService = _services.Get<CameraService>();
            RunStatisticsService runStatisticsService = _services.Get<RunStatisticsService>();
            UIRoot uiRoot = _services.Get<UIRoot>();
            CollectibleService collectibleService = _services.Get<CollectibleService>();
            CaptureZoneService captureZoneService = _services.Get<CaptureZoneService>();
            CrowdRemoveMembersSystem crowdRemoveMembersSystem = _services.Get<CrowdRemoveMembersSystem>();
            
            context
                .AddSystem(new CrowdMultiplySystem())
                .AddSystem(new CrowdAddMembersSystem(crowdFactory, crowdTriangleFormationService))
                .AddSystem(crowdRemoveMembersSystem)
                .AddSystem(new OrganizeCrowdSystem(
                    crowdTriangleFormationService,
                    gameStateMachine,
                    runStatisticsService))
                .AddSystem(new CrowdShootingSystem(
                    projectileFactory,
                    gameStateMachine,
                    runStatisticsService,
                    levelService))
                .AddSystem(crowdEvolutionSystem)
                .AddSystem(new CrowdBoundaryCalculationSystem())
                .AddSystem(new CrowdMovementCalculationSystem(
                    inputService,
                    gameStateMachine,
                    runStatisticsService))
                .AddSystem(new CrowdMovementSystem(
                    gameStateMachine,
                    runStatisticsService,
                    uiRoot
                ))
                // visual
                .AddSystem(new CrowdMemberHealthDisplaySystem())
                //
                .AddSystem(new CrowdMembersDeathSystem())

                // game state
                .AddSystem(new FinishTriggerSystem(
                    levelService,
                    cameraService,
                    collectibleService,
                    captureZoneService))
                .AddSystem(new GameEndEventSystem(
                    gameStateMachine,
                    levelService))
                ;
        }
    }
}