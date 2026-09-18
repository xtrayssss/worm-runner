using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.MovementFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.MovementFeature
{
    public sealed class MovementFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new TransformToPositionSyncSystem());
        }
    }
}