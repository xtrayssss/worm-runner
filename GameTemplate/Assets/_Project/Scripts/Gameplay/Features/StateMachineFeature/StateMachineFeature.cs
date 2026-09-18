using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature
{
    public sealed class StateMachineFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new UpdateStateMachineSystem());
        }
    }
}