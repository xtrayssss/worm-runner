using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature
{
    public sealed class StatusVisualsFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new ApplyPoisonVisualsSystem())
                .AddSystem(new ApplyFreezeVisualsSystem())
                .AddSystem(new UnapplyPoisonVisualsSystem())
                .AddSystem(new UnapplyFreezeVisualsSystem())
                ;
        }
    }
}