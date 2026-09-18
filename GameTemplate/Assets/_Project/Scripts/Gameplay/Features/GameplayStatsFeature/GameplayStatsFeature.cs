using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Systems;

namespace _Project.Scripts.Gameplay.Features.GameplayStatsFeature
{
    public sealed class GameplayStatsFeature : IFeature
    {
        public void Configure(FeatureContext context)
        {
            context
                .AddSystem(new ApplySpeedFromStatSystem())
                .AddSystem(new ApplyCooldownFromStatSystem())
                .AddSystem(new ApplyDamageFromStatSystem())
                .AddSystem(new ApplyHealthFromStatSystem())
                .AddSystem(new ApplyShootingRangeFromStatSystem())
                .AddSystem(new ApplyHealthRegenFromStatSystem())
                .AddSystem(new ApplyDefenseRegenFromStatSystem())
                .AddSystem(new ApplyFreezeChanceFromStatSystem())
                .AddSystem(new ApplyBaseHealthBonusFromStatSystem())
                .AddSystem(new ApplyBaseDefenseBonusFromStatSystem())
                ;
        }
    }
}