using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.StealthFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.StealthFeature
{
    public sealed class StealthFeature : IFeature
    {
        private readonly AllServices _services;
        
        public StealthFeature(AllServices services) => 
            _services = services;
        
        public void Configure(FeatureContext context)
        {
            AudioService audioService = _services.Get<AudioService>();
            
            context
                .AddSystem(new PatrolGuardSystem())
                .AddSystem(new WatchTowerSystem())
                .AddSystem(new VisionDetectionSystem(audioService))
                ;
        }
    }
}