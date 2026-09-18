using _Project.Scripts.Gameplay.Features.FeatureTree;

namespace _Project.Scripts.Gameplay.Features.CollisionFeature
{
    public sealed class CollisionFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new CollisionCleanupSystem());
        }
    }
}