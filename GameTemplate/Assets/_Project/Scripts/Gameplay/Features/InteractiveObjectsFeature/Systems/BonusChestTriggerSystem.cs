using System.Linq;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.RewardFeature.Components;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BonusChestTriggerSystem : ISystem
    {
        public World World { get; set; }

        private Filter _bonusChests;
        private Event<BonusChestOpenedEvent> _chestOpenedEvent;
        private Filter _crowdMembers;
        
        private readonly EnhancementService _enhancementService;
        private readonly RunStatisticsService _runStatisticsService;
        private readonly RewardService _rewardService;

        public BonusChestTriggerSystem(
            RunStatisticsService runStatisticsService,
            RewardService rewardService,
            EnhancementService enhancementService)
        {
            _runStatisticsService = runStatisticsService;
            _rewardService = rewardService;
            _enhancementService = enhancementService;
        }

        public void OnAwake()
        {
            _bonusChests = World.Filter
                .With<BonusChestTag>()
                .With<EntityViewLink>()
                .With<ActiveTrigger>()
                .Without<BonusChestTriggeredMarker>()
                .Build();

            _crowdMembers = World.Filter
                .With<MilitaryCrowdMemberTag>()
                .With<EntityViewLink>()
                .Build();

            _chestOpenedEvent = World.GetEvent<BonusChestOpenedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity chest in _bonusChests)
            {
                ref readonly ActiveTrigger activeTrigger = ref chest.GetComponent<ActiveTrigger>();

                if (activeTrigger.Triggers.Count == 0)
                    continue;

                bool shouldTrigger = CheckTriggerConditions(activeTrigger);

                if (shouldTrigger)
                    OpenBonusChest(chest);
            }
        }

        private void OpenBonusChest(Entity chest)
        {
            chest.AddComponent<BonusChestTriggeredMarker>();

            ref readonly EntityViewLink viewLink = ref chest.GetComponent<EntityViewLink>();

            ref readonly RewardsLink rewardsLink = ref chest.GetComponent<RewardsLink>();

            Rewards rewards = RewardsBuilder.Create()
                .FromExistingRewards(rewardsLink.Value)
                .Build();

            float rewardsMultiplier = _enhancementService.Upgrades[UpgradeType.INCOME].CurrentValue;
            _rewardService.GrantRewards(rewards, multiplier: rewardsMultiplier);
            _runStatisticsService.AddRewards(rewards);
            _runStatisticsService.SetBonusChestTriggered(true);

            BonusChestView chestView = (BonusChestView)viewLink.View;
            chestView.OnChestOpened += OnChestOpenAnimationCompleted;
            chestView.OpenChest().Forget();

            foreach (Entity member in _crowdMembers)
            {
                ref readonly EntityViewLink memberViewLink = ref member.GetComponent<EntityViewLink>();
                CrowdMemberView memberView = (CrowdMemberView)memberViewLink.View;
                memberView.StopJump();
            }
        }

        private void OnChestOpenAnimationCompleted() =>
            _chestOpenedEvent.NextFrame(new BonusChestOpenedEvent());

        private static bool CheckTriggerConditions(in ActiveTrigger activeTrigger)
        {
            return activeTrigger.Triggers.Any(static triggerInfo =>
                !triggerInfo.Other.Entity.IsNullOrDisposed() &&
                triggerInfo.Other.Entity.Has<MilitaryCrowdMemberTag>());
        }

        public void Dispose()
        {
        }
    }
}