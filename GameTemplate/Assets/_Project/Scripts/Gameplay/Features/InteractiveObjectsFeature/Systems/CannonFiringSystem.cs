using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CannonFiringSystem : ISystem
    {
        public World World { get; set; }

        private Filter _cannons;

        private readonly ProjectileFactory _projectileFactory;
        private readonly GameStateMachine _gameStateMachine;

        public CannonFiringSystem(ProjectileFactory projectileFactory, GameStateMachine gameStateMachine)
        {
            _projectileFactory = projectileFactory;
            _gameStateMachine = gameStateMachine;
        }

        public void OnAwake()
        {
            _cannons = World.Filter
                .With<CannonTag>()
                .With<CannonWeapon>()
                .With<EntityViewLink>()
                .With<AttackCooldown>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState)
                return;
            
            foreach (Entity cannon in _cannons)
            {
                ref CannonWeapon weapon = ref cannon.GetComponent<CannonWeapon>();
                ref AttackCooldown attackCooldown = ref cannon.GetComponent<AttackCooldown>();

                attackCooldown.CooldownTimer -= deltaTime;

                if (attackCooldown.CooldownTimer <= 0f)
                {
                    FireCannon(cannon, weapon);
                    attackCooldown.CooldownTimer = attackCooldown.Cooldown;
                }
            }
        }

        private void FireCannon(Entity cannon, in CannonWeapon weapon)
        {
            Vector3 firePosition = weapon.ShootPoint.position;

            Entity projectile = _projectileFactory.CreateProjectile(
                ProjectileId.CANNON_BALL,
                firePosition,
                Vector3.forward * -1,
                cannon);

            ref EntityViewLink projectileView = ref projectile.GetComponent<EntityViewLink>();
            LayerUtils.SetLayerRecursively(projectileView.View.gameObject, LayerUtils.ENEMY_PROJECTILE_LAYER);

            ref readonly EntityViewLink cannonViewLink = ref cannon.GetComponent<EntityViewLink>();

            CannonView cannonView = (CannonView)cannonViewLink.View;
           
            cannonView.PlayFireEffect();
        }

        public void Dispose()
        {
        }
    }
}