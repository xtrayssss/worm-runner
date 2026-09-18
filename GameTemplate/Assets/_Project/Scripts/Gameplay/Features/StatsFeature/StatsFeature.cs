using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatsFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.StatsFeature
{
    public sealed class StatsFeature : IFeature
    {
        private readonly AllServices _services;

        public StatsFeature(AllServices services) =>
            _services = services;

        public void Configure(FeatureContext context)
        {
            StatsService statService = _services.Get<StatsService>();

            context
                .AddSystem(new DestroyModifiersWithoutTargetSystem())
                .AddSystem(new DestroyStatChangedEventsSystem())
                .AddSystem(new DestroyRemovedModifiersSystem())
                .AddSystem(new ClearStatModifiersSystem(statService))
                .AddSystem(new RequestRemoveModifierSystem())
                //
                .AddSystem(new PrepareStatModifiersSystem())
                .AddSystem(new AddStatModifierSystem(statService))
                .AddSystem(new RemoveStatModifierSystem(statService))
                //
                .AddSystem(new MarkAppliedToModifiersSystem())
                .AddSystem(new RemoveStatModificationRequestsSystem())
                .AddSystem(new RemoveCustomMinValueOnRemoveSystem())
                ;
        }
    }
}