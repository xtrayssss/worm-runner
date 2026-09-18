using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileDestroySystem : ISystem
    {
        private readonly ProjectileFactory _projectileFactory;
        public World World { get; set; }

        private Filter _projectiles;

        public ProjectileDestroySystem(ProjectileFactory projectileFactory) =>
            _projectileFactory = projectileFactory;

        public void OnAwake()
        {
            _projectiles = World.Filter
                .With<ProjectileTag>()
                .With<ProjectileDeathMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity projectile in _projectiles)
            {
                ref readonly EntityViewLink viewLink = ref projectile.GetComponent<EntityViewLink>();
                _projectileFactory.ReturnToPool((ProjectileView)viewLink.View, (ProjectileView)viewLink.Prefab);
                World.RemoveEntity(projectile);
            }
        }

        public void Dispose()
        {
        }
    }
}