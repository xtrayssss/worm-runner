using _Project.Scripts.Gameplay.Features.CollisionFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature;
using _Project.Scripts.Gameplay.Features.DamageFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.FeatureTree;
using _Project.Scripts.Gameplay.Features.GameplayStatsFeature;
using _Project.Scripts.Gameplay.Features.GameTimeFeature;
using _Project.Scripts.Gameplay.Features.InputFeature.Systems;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.LifeForceFeature;
using _Project.Scripts.Gameplay.Features.MovementFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StealthFeature;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameFeature : IFeature
    {
        private readonly AllServices _services;

        public GameFeature(AllServices services)
        {
            _services = services;
        }

        public void Configure(FeatureContext context)
        {
            context
                .AddFeature(new InputFeature())
                .AddFeature(new MovementFeature())
                .AddFeature(new CollisionFeature())
                .AddFeature(new CrowdFeature(_services))
                .AddFeature(new UpgradesFeature(_services))
                .AddFeature(new ProjectileFeature(_services))
                .AddFeature(new InteractiveObjectsFeature(_services))
                .AddFeature(new StealthFeature(_services))
                .AddFeature(new DamageFeature(_services))
                .AddFeature(new StatsFeature(_services))
                .AddFeature(new GameplayStatsFeature())
                .AddFeature(new LifeForceFeature())
                .AddFeature(new GameTimeFeature(_services))

                // .AddFeature(new StateMachineFeature())
                // .AddFeature(new ExperienceFeature())
                // .AddFeature(new LevelUpFeature(_services))
                // .AddFeature(new AbilityFeature(_services))
                // .AddFeature(new CooldownFeature(_servicesС))
                // .AddFeature(new GameplayAbilitiesFeature(_services))
                // .AddFeature(new MovementFeature())
                // .AddFeature(new KnockbackFeature())
                // .AddFeature(new DamageFeature(_services))
                // .AddFeature(new CriticalHitFeature(_services))
                // .AddFeature(new DefenseFeature(_services))
                // .AddFeature(new StatusFeature(_services))
                // .AddFeature(new LifeForceFeature())
                // .AddFeature(new DamageVignetteFeature())
                // .AddFeature(new AnimationFeature())
                // //.AddFeature(new FakeShadowFeature())
                // .AddFeature(new DeathFeature())
                // .AddFeature(new CollisionFeature())
                ;
        }
    }
}