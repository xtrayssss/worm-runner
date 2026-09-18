using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileMovementSystem : IFixedSystem
    {
        public World World { get; set; }

        private Filter _projectiles;

        public void OnAwake()
        {
            _projectiles = World.Filter
                .With<ProjectileTag>()
                .With<MovementSpeedFactor>()
                .With<RigidbodyLink>()
                .With<MovementDirection>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity projectile in _projectiles)
            {
                ref readonly MovementSpeedFactor speed = ref projectile.GetComponent<MovementSpeedFactor>();
                ref RigidbodyLink rigidbodyLink = ref projectile.GetComponent<RigidbodyLink>();
                ref readonly MovementDirection movementDirection = ref projectile.GetComponent<MovementDirection>();

                Vector3 deltaMovement = movementDirection.Value * (speed.CurrentSpeed * deltaTime);

                rigidbodyLink.Value.MovePosition(rigidbodyLink.Value.position + deltaMovement);
            }
        }

        public void Dispose()
        {
        }
    }
}