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
    public sealed class CashStoneDestructionSystem : ISystem
    {
        private readonly RewardService _rewardService;
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvent;
        private readonly EnhancementService _enhancementService;
        private readonly RunStatisticsService _runStatisticsService;

        public CashStoneDestructionSystem(
            RewardService rewardService,
            EnhancementService enhancementService,
            RunStatisticsService runStatisticsService)
        {
            _rewardService = rewardService;
            _enhancementService = enhancementService;
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
                    !damagedEvent.Damageable.Has<CashStoneTag>() ||
                    !damagedEvent.Damageable.Has<ZeroHealthMarker>() ||
                    damagedEvent.Damageable.Has<CashStoneDestroyingMarker>())
                    continue;

                Entity cashStone = damagedEvent.Damageable;

                cashStone.AddComponent<CashStoneDestroyingMarker>();

                EntityViewLink viewLink = cashStone.GetComponent<EntityViewLink>();

                ref ColliderLink colliderLink = ref cashStone.GetComponent<ColliderLink>();
                colliderLink.Value.enabled = false;

                ref readonly RewardsLink rewardsLink = ref cashStone.GetComponent<RewardsLink>();

                Rewards rewards = RewardsBuilder.Create()
                    .FromExistingRewards(rewardsLink.Value)
                    .Build();

                float rewardsMultiplier = _enhancementService.Upgrades[UpgradeType.INCOME].CurrentValue;
                _rewardService.GrantRewards(rewards, multiplier: rewardsMultiplier);
                _runStatisticsService.AddRewards(rewards);

                CashStoneView cashStoneView = (CashStoneView)viewLink.View;
                
                cashStoneView
                    .PlayDestructionVFX(scale: 3f)
                    .PlayDestructionAnimation()
                    .OnComplete(
                        cashStoneView,
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