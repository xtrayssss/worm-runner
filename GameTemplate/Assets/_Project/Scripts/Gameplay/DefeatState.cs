using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.AudioFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay
{
    public sealed class DefeatState : State<GameStateMachine.Context>
    {
        private readonly AudioService _audioService;
        private readonly WindowService _windowsService;
        private readonly RewardService _rewardService;
        private readonly GameStateMachine _gameStateMachine;
        private readonly RunStatisticsService _runStatisticsService;
        private readonly AdvertisementService _advertisementService;
        private readonly LevelService _levelService;

        private GameResultPopup _gameResultPopup;

        public DefeatState(AllServices services)
        {
            _audioService = services.Get<AudioService>();
            _rewardService = services.Get<RewardService>();
            _gameStateMachine = services.Get<GameStateMachine>();
            _runStatisticsService = services.Get<RunStatisticsService>();
            _advertisementService = services.Get<AdvertisementService>();
            _windowsService = services.Get<WindowService>();
            _levelService = services.Get<LevelService>();
        }

        public override UniTask Enter(GameStateMachine.Context ctx)
        {
            _audioService.PlaySpecialSound(AudioId.Sfx.Gameplay.LOSE);
            ShowGameResultPopup();
            return UniTask.CompletedTask;
        }

        public override UniTask Exit(GameStateMachine.Context ctx)
        {
            return UniTask.CompletedTask;
        }

        private void ShowGameResultPopup()
        {
            _gameResultPopup = _windowsService.OpenTemporaryPopup<GameResultPopup>(WindowId.GAME_RESULT_POPUP);

            RunStatistics currentRun = _runStatisticsService.CurrentRun;
            
            _gameResultPopup.Construct(
                isVictory: false,
                collectedMoney: currentRun.CollectedMoney,
                distanceTraveled: currentRun.DistanceTraveled,
                crowdMembersLost: currentRun.CrowdMembersLost,
                onDoubleRewardGranted: GrantDoubleReward,
                advertisementService: _advertisementService,
                onClosed: OnGameResultPopupClosed
            );
            
            _gameResultPopup.Show();
        }

        private void GrantDoubleReward()
        {
            Rewards rewards = RewardsBuilder.Create()
                .WithMoney(_runStatisticsService.CurrentRun.CollectedMoney)
                .Build();

            _rewardService.GrantRewards(rewards);
            _runStatisticsService.AddRewards(rewards);

            _gameResultPopup.UpdateCollectedMoney(_runStatisticsService.CurrentRun.CollectedMoney);
        }

        private void OnGameResultPopupClosed()
        {
            ClosePopupAsync().Forget();

            return;

            async UniTaskVoid ClosePopupAsync()
            {
                await _windowsService.CloseTemporaryPopup(_gameResultPopup);
                await _levelService.ReloadCurrentLevel(withFadeAnimation: true);
                _gameStateMachine.Fire(GameplayEvent.ENTER_MENU);
            }
        }
    }
}