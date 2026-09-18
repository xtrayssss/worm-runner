using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Systems;
using _Project.Scripts.Gameplay.Features.FeatureTree;

namespace _Project.Scripts.Gameplay.Features.EnhancementFeature
{
    public sealed class UpgradesFeature : IFeature
    {
        private readonly AllServices _services;

        public UpgradesFeature(AllServices services) => 
            _services = services;

        public void Configure(FeatureContext context)
        {
            EnhancementService enhancementService = _services.Get<EnhancementService>();
            CurrencyService currencyService = _services.Get<CurrencyService>();
            
            context
                .AddSystem(new ApplyUpgradesSystem(enhancementService, currencyService));
        }
    }
}