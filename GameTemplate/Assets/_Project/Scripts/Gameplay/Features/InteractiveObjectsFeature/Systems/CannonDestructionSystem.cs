using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
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
    public sealed class CannonDestructionSystem : ISystem
    {
        private readonly RewardService _rewardService;
        private readonly EnhancementService _upgradeService;
        private readonly RunStatisticsService _runStatisticsService;

        public World World { get; set; }
        private Event<DamagedEvent> _damagedEvent;

        public CannonDestructionSystem(
            RewardService rewardService,
            EnhancementService upgradeService,
            RunStatisticsService runStatisticsService)
        {
            _rewardService = rewardService;
            _upgradeService = upgradeService;
            _runStatisticsService = runStatisticsService;
        }

        public void OnAwake()
        {
            _damagedEvent = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvent.publishedChanges)
            {
                if (damagedEvent.Damageable.IsNullOrDisposed() ||
                    !damagedEvent.Damageable.Has<CannonTag>() ||
                    !damagedEvent.Damageable.Has<ZeroHealthMarker>() ||
                    damagedEvent.Damageable.Has<CannonDestroyingMarker>())
                    continue;

                Entity canon = damagedEvent.Damageable;

                canon.AddComponent<CannonDestroyingMarker>();

                EntityViewLink viewLink = canon.GetComponent<EntityViewLink>();

                ref ColliderLink colliderLink = ref canon.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                ref readonly RewardsLink rewardsLink = ref canon.GetComponent<RewardsLink>();

                Rewards rewards = RewardsBuilder.Create()
                    .FromExistingRewards(rewardsLink.Value)
                    .Build();

                float rewardsMultiplier = _upgradeService.Upgrades[UpgradeType.INCOME].CurrentValue;
                _rewardService.GrantRewards(rewards, multiplier: rewardsMultiplier);
                _runStatisticsService.AddRewards(rewards);

                CannonView cannonView = (CannonView)viewLink.View;
               
                cannonView
                    .PlayDestructionVFX(scale: 2f)
                    .PlayDestructionAnimation()
                    .OnComplete(
                        cannonView,
                        static view =>
                        {
                            Object.Destroy(view.gameObject);

                            if (!view.Entity.IsNullOrDisposed())
                                World.Default.RemoveEntity(view.Entity);
                        });
            }
        }

        public void Dispose()
        {
        }
    }
}