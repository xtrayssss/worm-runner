using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileLifetimeSystem : ISystem
    {
        public World World { get; set; }
        private Filter _projectiles;

        public void OnAwake()
        {
            _projectiles = World.Filter
                .With<ProjectileTag>()
                .With<DamageImpactEvent>()
                .Without<ProjectileDeathMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity projectile in _projectiles)
                projectile.AddComponent<ProjectileDeathMarker>();
        }

        public void Dispose()
        {
        }
    }
}