using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RiflemanShootingSystem : ISystem
    {
        public World World { get; set; }
        private Filter _enemies;

        private readonly GameStateMachine _gameStateMachine;
        private readonly ProjectileFactory _projectileFactory;

        public RiflemanShootingSystem(
            ProjectileFactory projectileFactory,
            GameStateMachine gameStateMachine)
        {
            _projectileFactory = projectileFactory;
            _gameStateMachine = gameStateMachine;
        }

        public void OnAwake()
        {
            _enemies = World.Filter
                .With<EnemyTag>()
                .With<AttackCooldown>()
                .With<EntityViewLink>()
                .With<RiflemanEnemyTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState)
                return;

            foreach (Entity enemy in _enemies)
            {
                ref AttackCooldown attackCooldown = ref enemy.GetComponent<AttackCooldown>();

                attackCooldown.CooldownTimer -= deltaTime;

                if (attackCooldown.CooldownTimer <= 0f)
                {
                    ShootProjectile(enemy);
                    attackCooldown.CooldownTimer = attackCooldown.Cooldown;
                }
            }
        }

        private void ShootProjectile(Entity enemy)
        {
            ref readonly EntityViewLink viewLink = ref enemy.GetComponent<EntityViewLink>();

            EnemyView enemyView = (EnemyView)viewLink.View;

            Vector3 shootPosition = enemyView.ShootPoint.position;
            Vector3 shootDirection = -Vector3.forward;

            Entity projectile = _projectileFactory.CreateProjectile(
                ProjectileId.ENEMY_BULLET,
                shootPosition,
                shootDirection,
                enemy);

            ref EntityViewLink projectileView = ref projectile.GetComponent<EntityViewLink>();
            LayerUtils.SetLayerRecursively(projectileView.View.gameObject, LayerUtils.ENEMY_PROJECTILE_LAYER);

            enemyView.PlayShootEffect();
        }

        public void Dispose()
        {
        }
    }
}