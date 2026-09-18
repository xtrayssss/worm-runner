using _Project.Scripts.Gameplay.Features.DeathFeature.Systems;
using _Project.Scripts.Gameplay.Features.FeatureTree;

namespace _Project.Scripts.Gameplay.Features.DeathFeature
{
    public sealed class DeathFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new SelfDestructSystem());
        }
    }
}