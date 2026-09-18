using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.LifeForceFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.LifeForceFeature
{
    public sealed class LifeForceFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new HealthDisplaySystem())
                ;
        }
    }
}