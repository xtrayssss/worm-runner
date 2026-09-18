using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay
{
    public sealed class VictoryState : State<GameStateMachine.Context>
    {
        private readonly AudioService _audioService;
        private readonly WindowService _windowsService;
        private readonly GameStateMachine _gameStateMachine;
        private readonly RunStatisticsService _runStatisticsService;
        private readonly LevelService _levelService;
        private readonly RewardService _rewardService;
        private readonly AdvertisementService _advertisementService;
        private readonly EnhancementService _enhancementService;
        private readonly ConfigsService _configsService;

        private readonly UpgradeType[] _upgradesToReset =
        {
            UpgradeType.ATTACK,
            UpgradeType.INCOME,
            UpgradeType.POPULATION
        };

        private GameResultPopup _gameResultPopup;
        private ChaptersPopup _chaptersPopup;

        public VictoryState(AllServices services)
        {
            _audioService = services.Get<AudioService>();
            _windowsService = services.Get<WindowService>();
            _gameStateMachine = services.Get<GameStateMachine>();
            _runStatisticsService = services.Get<RunStatisticsService>();
            _levelService = services.Get<LevelService>();
            _rewardService = services.Get<RewardService>();
            _advertisementService = services.Get<AdvertisementService>();
            _enhancementService = services.Get<EnhancementService>();
            _configsService = services.Get<ConfigsService>();
        }

        public override UniTask Enter(GameStateMachine.Context ctx)
        {
            _levelService.AdvanceToNextLevel();

            bool isNewChapter = _levelService.IsNewChapter(_levelService.CurrentLevelIndex);

            if (isNewChapter)
                _enhancementService.ResetUpgrades(_upgradesToReset);

            if (isNewChapter)
                ShowChaptersPopup();
            else
                ShowGameResultPopup();

            _audioService.PlaySpecialSound(AudioId.Sfx.Gameplay.WINNING);

            return UniTask.CompletedTask;
        }

        private void ShowGameResultPopup()
        {
            _gameResultPopup = _windowsService.OpenTemporaryPopup<GameResultPopup>(WindowId.GAME_RESULT_POPUP);

            _gameResultPopup.Construct(
                isVictory: true,
                collectedMoney: _runStatisticsService.CurrentRun.CollectedMoney,
                distanceTraveled: _runStatisticsService.CurrentRun.DistanceTraveled,
                crowdMembersLost: _runStatisticsService.CurrentRun.CrowdMembersLost,
                onDoubleRewardGranted: GrantDoubleReward,
                advertisementService: _advertisementService,
                onClosed: OnGameResultPopupClosed
            );

            _gameResultPopup.Show();
        }

        private void OnGameResultPopupClosed() =>
            ProceedToNextLevel(_gameResultPopup).Forget();

        private void GrantDoubleReward()
        {
            Rewards rewards = RewardsBuilder.Create()
                .WithMoney(_runStatisticsService.CurrentRun.CollectedMoney)
                .Build();

            _rewardService.GrantRewards(rewards);
            _runStatisticsService.AddRewards(rewards);

            _gameResultPopup.UpdateCollectedMoney(_runStatisticsService.CurrentRun.CollectedMoney);
        }

        private void ShowChaptersPopup()
        {
            _chaptersPopup = _windowsService.OpenTemporaryPopup<ChaptersPopup>(WindowId.CHAPTERS_POPUP);

            _chaptersPopup.Construct(
                levelsDatabase: _configsService.GetLevelsDatabase(),
                levelService: _levelService,
                onClosed: OnChaptersPopupClosed);

            _chaptersPopup.ShowAsync();
        }

        private void OnChaptersPopupClosed() =>
            ProceedToNextLevel(_chaptersPopup).Forget();

        private async UniTaskVoid ProceedToNextLevel(BaseWindow popup)
        {
            await _windowsService.CloseTemporaryPopup(popup);
            await _levelService.ReloadCurrentLevel(withFadeAnimation: true);
            _gameStateMachine.Fire(GameplayEvent.ENTER_MENU);
        }
    }
}