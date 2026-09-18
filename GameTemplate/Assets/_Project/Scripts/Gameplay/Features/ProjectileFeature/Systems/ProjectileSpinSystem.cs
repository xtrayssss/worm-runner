using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileSpinSystem : ISystem
    {
        public World World { get; set; }

        private Filter _rotatingProjectiles;

        public void OnAwake()
        {
            _rotatingProjectiles = World.Filter
                .With<ProjectileTag>()
                .With<EntityViewLink>()
                .With<ConstantSpin>()
                .Without<ProjectileDeathMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity projectile in _rotatingProjectiles)
            {
                ref readonly EntityViewLink entityViewLink = ref projectile.GetComponent<EntityViewLink>();
                ref ConstantSpin constantSpin = ref projectile.GetComponent<ConstantSpin>();

                Transform transform = entityViewLink.View.transform;
                constantSpin.CurrentAngle += constantSpin.RotationSpeed * deltaTime;
                ApplyConstantSpin(transform, in constantSpin);
            }
        }

        private void ApplyConstantSpin(Transform transform, in ConstantSpin settings)
        {
            transform.rotation = Quaternion.Euler(
                settings.RotationAxis.x * settings.CurrentAngle,
                settings.RotationAxis.y * settings.CurrentAngle,
                settings.RotationAxis.z * settings.CurrentAngle
            );
        }

        public void Dispose()
        {
        }
    }
}