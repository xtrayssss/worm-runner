using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Systems;
using _Project.Scripts.Gameplay.Features.FeatureTree;

namespace _Project.Scripts.Gameplay.Features.DamageFeature
{
    public sealed class DamageFeature : IFeature
    {
        private readonly AllServices _services;

        public DamageFeature(AllServices services) =>
            _services = services;

        public void Configure(FeatureContext context)
        {
            ConfigsService configs = _services.Get<ConfigsService>();

            context
                .AddSystem(new RemoveDamageImpactEventsSystem())
                .AddSystem(new DamageImpactSystem())
                .AddSystem(new DamagePopupSystem(configs))
                ;
        }
    }
}