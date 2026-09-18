using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Systems;
using _Project.Scripts.Gameplay.Features.StatusFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.StatusFeature
{
    public sealed class StatusFeature : IFeature
    {
        private readonly AllServices _services;

        public StatusFeature(AllServices services) =>
            _services = services;

        public void Configure(FeatureContext context)
        {
            StatusApplier statusApplier = _services.Get<StatusApplier>();
            StatusFactory statusFactory = _services.Get<StatusFactory>();
            StatsService statsService = _services.Get<StatsService>();

            context
                .AddSystem(new CreateStatusSystem(statusFactory))
                .AddSystem(new DestroyStatusesWithoutTargetSystem())
                .AddSystem(new DestroyDeadStatusesSystem())
                .AddSystem(new StatusLifetimeSystem())
                .AddSystem(new RemoveStatusSystem())
                //
                .AddFeature(new StatusVisualsFeature.StatusVisualsFeature())
                //
                .AddSystem(new SyncStatusPositionToTargetPositionSystem())
                .AddSystem(new PeriodicStatusSystem(statsService))
                //
                .AddSystem(new FreezeChanceStatusSystem(statusApplier))
                .AddSystem(new ApplyFreezeStatusSystem(statsService))
                .AddSystem(new UnapplyFreezeStatusSystem(statsService))
                //
                .AddSystem(new RemoveAppliedEventsOnStatusesSystem())
                .AddSystem(new RemoveStatusCreatedEventsSystem())
                ;
        }
    }
}