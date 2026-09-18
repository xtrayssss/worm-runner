using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.ProjectileFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Services;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CrowdShootingSystem : ISystem
    {
        public World World { get; set; }

        private readonly ProjectileFactory _projectileFactory;
        private Filter _shootingMembers;
        private Filter _crowds;
        private readonly GameStateMachine _gameStateMachine;
        private readonly RunStatisticsService _runStatisticsService;
        private readonly LevelService _levelService;

        public CrowdShootingSystem(
            ProjectileFactory projectileFactory,
            GameStateMachine gameStateMachine,
            RunStatisticsService runStatisticsService,
            LevelService levelService)
        {
            _projectileFactory = projectileFactory;
            _gameStateMachine = gameStateMachine;
            _runStatisticsService = runStatisticsService;
            _levelService = levelService;
        }

        public void OnAwake()
        {
            _shootingMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<CrowdMemberEvolution>()
                .With<EntityViewLink>()
                .With<AttackCooldown>()
                .Without<CrowdMemberDyingMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState ||
                _runStatisticsService.CurrentRun.IsBonusChestTriggered ||
                _levelService.CurrentLevel.LevelMode == LevelMode.STEALTH)
                return;

            foreach (Entity member in _shootingMembers)
            {
                ref AttackCooldown cooldown = ref member.GetComponent<AttackCooldown>();

                cooldown.CooldownTimer -= deltaTime;

                if (cooldown.CooldownTimer <= 0f)
                {
                    ShootProjectile(member);
                    cooldown.CooldownTimer = cooldown.Cooldown;
                }
            }
        }

        private void ShootProjectile(Entity member)
        {
            ref readonly EntityViewLink viewLink = ref member.GetComponent<EntityViewLink>();
            MilitaryCrowdMemberView view = (MilitaryCrowdMemberView)viewLink.View;

            Vector3 shootPosition = view.ShootPoint.position;
            Vector3 shootDirection = Vector3.forward;

            Entity projectile = _projectileFactory.CreateProjectile(
                ProjectileId.BULLET,
                shootPosition,
                shootDirection,
                member);

            ref EntityViewLink projectileViewLink = ref projectile.GetComponent<EntityViewLink>();
            LayerUtils.SetLayerRecursively(projectileViewLink.View.gameObject, LayerUtils.MEMBER_PROJECTILE_LAYER);

            ProjectileView projectileView = (ProjectileView)projectileViewLink.View;
            projectileView.StartAnimation();
            
            view.PlayShootEffect();
        }

        public void Dispose()
        {
        }
    }
}