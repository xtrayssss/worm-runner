using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.RewardFeature.Components;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnemyDestructionSystem : ISystem
    {
        public World World { get; set; }
        private Event<DamagedEvent> _damagedEvents;

        private readonly SoulFactory _soulFactory;
        private readonly RewardService _rewardService;
        private readonly EnhancementService _enhancementService;
        private readonly RunStatisticsService _runStatisticsService;

        public EnemyDestructionSystem(
            SoulFactory soulFactory,
            RewardService rewardService,
            EnhancementService enhancementService,
            RunStatisticsService runStatisticsService)
        {
            _soulFactory = soulFactory;
            _rewardService = rewardService;
            _enhancementService = enhancementService;
            _runStatisticsService = runStatisticsService;
        }

        public void OnAwake()
        {
            _damagedEvents = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvents.publishedChanges)
            {
                if (damagedEvent.Damageable.IsNullOrDisposed() ||
                    !damagedEvent.Damageable.Has<EnemyTag>() ||
                    damagedEvent.Damageable.Has<EnemyDestroyingMarker>() ||
                    !damagedEvent.Damageable.Has<ZeroHealthMarker>())
                    continue;

                Entity enemy = damagedEvent.Damageable;

                ref readonly EntityViewLink viewLink = ref enemy.GetComponent<EntityViewLink>();

                EnemyView enemyView = (EnemyView)viewLink.View;

                enemy.AddComponent<EnemyDestroyingMarker>();

                if (enemy.Has<KamikazeEnemy>())
                    enemyView.PlayKamikazeExplosionEffect();
                else
                    enemyView.PlayDestructionVFX();

                GrantRewards(enemy);

                enemyView
                    .PlayDestructionAnimation()
                    .OnComplete(
                        enemyView,
                        view =>
                        {
                            _soulFactory
                                .CreateSoul(view.transform.position, SoulType.ENEMY)
                                .PlaySoulAnimation();

                            Object.Destroy(view.gameObject);

                            if (!view.Entity.IsNullOrDisposed())
                                World.Default.RemoveEntity(view.Entity);
                        });
            }
        }

        private void GrantRewards(Entity enemy)
        {
            ref readonly RewardsLink rewardsLink = ref enemy.GetComponent<RewardsLink>();

            Rewards rewards = RewardsBuilder.Create()
                .FromExistingRewards(rewardsLink.Value)
                .Build();

            float rewardsMultiplier = _enhancementService.Upgrades[UpgradeType.INCOME].CurrentValue;
            _rewardService.GrantRewards(rewards, multiplier: rewardsMultiplier);
            _runStatisticsService.AddRewards(rewards);
        }

        public void Dispose()
        {
        }
    }
}