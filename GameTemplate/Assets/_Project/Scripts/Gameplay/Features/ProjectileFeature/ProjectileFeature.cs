using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Project.Scripts.Gameplay.Features.ProjectileFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature
{
    public sealed class ProjectileFeature : IFeature
    {
        private readonly AllServices _services;

        public ProjectileFeature(AllServices services) =>
            _services = services;

        public void Configure(FeatureContext context)
        {
            StatusApplier statusApplier = _services.Get<StatusApplier>();
            StatsService statsService = _services.Get<StatsService>();
            ProjectileFactory projectileFactory = _services.Get<ProjectileFactory>();
            VFXService vfxService = _services.Get<VFXService>();

            context
                .AddSystem(new ProjectileLifetimeSystem())
                .AddSystem(new ProjectileRangeLimitSystem())
                .AddSystem(new ProjectileDestroySystem(projectileFactory))
                .AddSystem(new ProjectileHitCountSystem())
                .AddSystem(new ProjectileCollisionSystem())
                .AddSystem(new ProjectileImpactSystem(statusApplier, statsService))
                .AddSystem(new ProjectileSpinSystem())
                .AddSystem(new ProjectileMovementSystem())
                ;
        }
    }
}