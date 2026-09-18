using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileImpactSystem : ISystem
    {
        public World World { get; set; }

        private Request<ProjectileImpactRequest> _impactRequest;
        private readonly StatusApplier _statusApplier;
        private readonly StatsService _statsService;

        public ProjectileImpactSystem(
            StatusApplier statusApplier,
            StatsService statsService)
        {
            _statusApplier = statusApplier;
            _statsService = statsService;
        }

        public void OnAwake() =>
            _impactRequest = World.GetRequest<ProjectileImpactRequest>();

        public void OnUpdate(float deltaTime)
        {
            foreach (ProjectileImpactRequest request in _impactRequest.Consume())
            {
                if (request.Projectile.Has<StatusSetups>())
                {
                    ref readonly StatusSetups statusSetups = ref request.Projectile.GetComponent<StatusSetups>();

                    foreach (StatusSetup statusSetup in statusSetups.Value)
                    {
                        _statusApplier.TryApplyStatus(
                            setup: statusSetup,
                            producer: request.Projectile,
                            target: request.Damageable,
                            enhancementSource: request.Projectile);
                    }
                }

                if (request.Projectile.Has<StatModifierSetups>())
                {
                    ref readonly StatModifierSetups statModifierSetups =
                        ref request.Projectile.GetComponent<StatModifierSetups>();

                    foreach (StatModifierSetup modifier in statModifierSetups.Value)
                    {
                        _statsService.CreateStatModifier(
                            target: request.Damageable,
                            modifierSetup: modifier,
                            statSource: request.Projectile,
                            point: request.Point,
                            producer: request.Projectile);
                    }
                }

                if (request.Projectile.Has<ExplosionOnImpact>())
                {
                    ref readonly ExplosionOnImpact explosion = ref request.Projectile.GetComponent<ExplosionOnImpact>();
                    
                    EntityViewLink viewLink = request.Projectile.GetComponent<EntityViewLink>();
                    ProjectileView view = (ProjectileView)viewLink.View;

                    view.PlayExplosionVFX(explosion.ExplosionVFX, request.Point);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}