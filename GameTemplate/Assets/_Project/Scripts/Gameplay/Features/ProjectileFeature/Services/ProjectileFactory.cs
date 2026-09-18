using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Configs;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.VFXFeature.Services;
using Scellecs.Morpeh;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Services
{
    public sealed class ProjectileFactory : IService
    {
        private Dictionary<ProjectileView, Queue<ProjectileView>> _projectilePools;
        private Transform _projectileContainer;
        private readonly ConfigsService _configs;
        private readonly VFXService _vfxService;

        public ProjectileFactory(ConfigsService configs, VFXService vfxService)
        {
            _configs = configs;
            _vfxService = vfxService;
        }

        public void InitializePool()
        {
            _projectilePools = new Dictionary<ProjectileView, Queue<ProjectileView>>();

            GameObject container = new GameObject("ProjectileContainer");
            _projectileContainer = container.transform;
        }

        public Entity CreateProjectile(ProjectileId id, Vector3 position, Vector3 direction, Entity shooter)
        {
            ProjectileConfig config = _configs.GetProjectileConfig(id);

            return CreateProjectile(config, position, direction, shooter);
        }

        private Entity CreateProjectile(ProjectileConfig config, Vector3 position,
            Vector3 direction, Entity shooter)
        {
            World world = World.Default;
            Entity projectile = world.CreateEntity();

            ProjectileView view = GetFromPool(config.ProjectilePrefab, position);
            view.Entity = projectile;
            view.transform.position = position;
            view.transform.rotation = Quaternion.Euler(view.transform.rotation.eulerAngles.x,
                Quaternion.LookRotation(direction).eulerAngles.y, view.transform.rotation.eulerAngles.z);
            view.gameObject.SetActive(true);

            projectile.AddComponent<ProjectileTag>();

            projectile.AddComponent<EntityViewLink>() = new EntityViewLink
            {
                View = view,
                Prefab = config.ProjectilePrefab
            };

            projectile.AddComponent<ProjectileDistanceTracker>() = new ProjectileDistanceTracker
            {
                StartPoint = position,
                MaxDistance = shooter.Has<ShootingRange>()
                    ? shooter.GetComponent<ShootingRange>().CurrentValue
                    : config.MaxDistance
            };

            projectile.AddComponent<MovementDirection>() = new MovementDirection
            {
                Value = direction
            };

            ref readonly Stats ownerStats = ref shooter.GetComponent<Stats>();

            ref Stats stats = ref projectile.AddComponent<Stats>();
            stats = new StatsBuilder()
                .SetStat(StatId.DAMAGE, ownerStats.Value[StatId.DAMAGE].CurrentValue)
                .SetStat(StatId.SPEED, config.Speed)
                .Build();

            projectile.AddComponent<Damage>() = new Damage
            {
                CurrentDamage = stats.Value[StatId.DAMAGE].CurrentValue
            };

            projectile.AddComponent<MovementSpeedFactor>() = new MovementSpeedFactor
            {
                CurrentSpeed = stats.Value[StatId.SPEED].CurrentValue,
            };

            projectile.AddComponent<Owner>().Value = shooter;

            projectile.AddComponent<CollisionLimit>() = new CollisionLimit
            {
                Limit = 1
            };

            projectile.AddComponent<RigidbodyLink>() = new RigidbodyLink
            {
                Value = view.Rigidbody
            };

            if (view.ImpactVFX != null)
            {
                projectile.AddComponent<ExplosionOnImpact>() = new ExplosionOnImpact
                {
                    ExplosionVFX = view.ImpactVFX
                };
            }

            ProjectileEnhancements.Create()
                .ForProjectile(projectile)
                .WithStatModifiersFrom(shooter)
                .WithStatusesFrom(shooter)
                // .WithCriticalFrom(shooter)
                //.WithGuaranteedCriticalFrom(shooter)
                .WithFreezeChance(shooter);

#if DEBUG
            projectile.AddComponent<EntityName>().Value = $"Projectile_{projectile.Id}";
#endif

            return projectile;
        }

        public void ReturnToPool(ProjectileView view, ProjectileView prefab)
        {
            view.gameObject.SetActive(false);
            view.transform.SetParent(_projectileContainer);
            view.transform.position = prefab.transform.position;
            view.transform.rotation = prefab.transform.rotation;
            view.transform.localScale = prefab.transform.localScale;
            view.Rigidbody.velocity = Vector3.zero;
            view.Rigidbody.angularVelocity = Vector3.zero;

            if (!_projectilePools.ContainsKey(prefab))
                _projectilePools[prefab] = new Queue<ProjectileView>();

            _projectilePools[prefab].Enqueue(view);
        }

        private ProjectileView GetFromPool(ProjectileView prefab, Vector3 spawnPosition)
        {
            if (!_projectilePools.ContainsKey(prefab))
                _projectilePools[prefab] = new Queue<ProjectileView>();

            Queue<ProjectileView> pool = _projectilePools[prefab];

            if (pool.Count > 0)
            {
                ProjectileView pooledObject = pool.Dequeue();
                if (pooledObject != null)
                {
                    return pooledObject;
                }
            }

            ProjectileView projectileView =
                Object.Instantiate(prefab, spawnPosition, prefab.transform.rotation, _projectileContainer);
            
            projectileView.Construct(_vfxService);

            return projectileView;
        }
    }
}