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

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MachineGunnerShootingSystem : ISystem
    {
        public World World { get; set; }
        
        private Filter _machineGunners;
        
        private readonly ProjectileFactory _projectileFactory;
        private readonly GameStateMachine _gameStateMachine;

        public MachineGunnerShootingSystem(ProjectileFactory projectileFactory, GameStateMachine gameStateMachine)
        {
            _projectileFactory = projectileFactory;
            _gameStateMachine = gameStateMachine;
        }
        
        public void OnAwake()
        {
            _machineGunners = World.Filter
                .With<EnemyTag>()
                .With<MachineGunnerEnemy>()
                .With<MachineGunnerBurstState>()
                .With<AttackCooldown>()
                .With<EntityViewLink>()
                .Build();
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState)
                return;
            
            foreach (Entity enemy in _machineGunners)
            {
                ref MachineGunnerEnemy gunnerData = ref enemy.GetComponent<MachineGunnerEnemy>();
                ref MachineGunnerBurstState burstState = ref enemy.GetComponent<MachineGunnerBurstState>();
                ref AttackCooldown cooldown = ref enemy.GetComponent<AttackCooldown>();
                
                if (burstState.IsBursting)
                {
                    burstState.BurstTimer -= deltaTime;
                    
                    if (burstState.BurstTimer <= 0f)
                    {
                        ShootProjectile(enemy);
                        burstState.CurrentBurstShot++;
                        
                        if (burstState.CurrentBurstShot >= gunnerData.BurstCount)
                        {
                            burstState.IsBursting = false;
                            burstState.CurrentBurstShot = 0;
                            cooldown.CooldownTimer = gunnerData.BurstCooldown;
                        }
                        else
                        {
                            burstState.BurstTimer = gunnerData.BurstInterval;
                        }
                    }
                }
                else
                {
                    cooldown.CooldownTimer -= deltaTime;
                    
                    if (cooldown.CooldownTimer <= 0f)
                    {
                        burstState.IsBursting = true;
                        burstState.CurrentBurstShot = 0;
                        burstState.BurstTimer = 0f;
                    }
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