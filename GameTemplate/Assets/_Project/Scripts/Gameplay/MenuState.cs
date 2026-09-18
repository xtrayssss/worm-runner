using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.RewardFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.TutorialFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
#if GAMEPUSH_ENABLED
using GamePush;
#endif

namespace _Project.Scripts.Gameplay
{
    [Serializable]
    public sealed class MenuState : State<GameStateMachine.Context>
    {
        private readonly UIRoot _uiRoot;
        private readonly CameraService _cameraService;
        private readonly LevelService _levelService;
        private readonly TutorialService _tutorialService;

        public MenuState(AllServices services)
        {
            _uiRoot = services.Get<UIRoot>();
            _cameraService = services.Get<CameraService>();
            _levelService = services.Get<LevelService>();
            _tutorialService = services.Get<TutorialService>(ignoreException: true);
        }

        public override UniTask Enter(GameStateMachine.Context ctx)
        {
#if GAMEPUSH_ENABLED
            GP_Game.GameReady();
#endif
            if (ctx.GameEndReason != GameEndingState.Reason.UNKNOWN)
                _uiRoot.GameWindow.MenuContent.CrowdMemberRewardButton.ResetState();

            _uiRoot.GameWindow.DistanceProgressBar.ResetProgress();
            _uiRoot.GameWindow.MenuContent.Show(_levelService.CurrentLevel.LevelMode);
            _uiRoot.GameWindow.SetGameState(GameState.MENU);
            _cameraService.RestoreFovToOriginal(duration: 0f);

            InitiateTutorial(ctx);

            return UniTask.CompletedTask;
        }

        private void InitiateTutorial(GameStateMachine.Context ctx)
        {
            if (_tutorialService == null || _tutorialService.IsTutorialCompleted())
                return;

            TutorialTemplate mainTemplate = _tutorialService.Templates[0];

            bool isPhaseInitiated = _tutorialService.InitiateTutorialIfNeeded(TutorialPhase.MENU_FIRST_UPGRADE);

            if (!isPhaseInitiated &&
                ctx.GameEndReason != GameEndingState.Reason.UNKNOWN &&
                !_uiRoot.GameWindow.MenuContent.CrowdMemberRewardButton.IsVisible)
            {
                _tutorialService.InitiateTutorialIfNeeded(TutorialPhase.MENU_PROGRESSION_BASICS);

                if (!_tutorialService.IsStepCompleted(mainTemplate.Id, stepIndex: 1))
                {
                    _uiRoot.GameWindow.MenuContent.CrowdMemberRewardButton
                        .SetRewardMode(RewardedAdButton.RewardMode.WITHOUT_AD);
                }
            }
            else if (!isPhaseInitiated && _tutorialService.IsStepCompleted(mainTemplate.Id, stepIndex: 1))
            {
                _tutorialService.ContinueTutorialFromPhase(TutorialPhase.MENU_PROGRESSION_BASICS, mainTemplate);
            }
        }

        public override UniTask Exit(GameStateMachine.Context ctx)
        {
            _uiRoot.GameWindow.MenuContent.Hide();

            return UniTask.CompletedTask;
        }
    }
}