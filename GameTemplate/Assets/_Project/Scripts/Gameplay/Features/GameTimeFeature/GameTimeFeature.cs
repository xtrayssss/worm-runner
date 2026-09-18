using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Services;
using _Project.Scripts.Gameplay.Features.GameTimeFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.GameTimeFeature
{
    public sealed class GameTimeFeature : IFeature
    {
        private readonly AllServices _services;

        public GameTimeFeature(AllServices services)
        {
            _services = services;
        }

        public void Configure(FeatureContext context)
        {
            GameTimeService gameTimeService = _services.Get<GameTimeService>();
            
            context
                .SetPausable(false)
                .AddSystem(new PauseSystem(gameTimeService, context.Tree));
        }
    }
}