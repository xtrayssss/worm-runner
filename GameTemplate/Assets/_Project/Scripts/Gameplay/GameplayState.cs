using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.StatisticsFeature;
using _Project.Scripts.Gameplay.Features.WindowFeature;
using Cysharp.Threading.Tasks;
using GamePush;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameplayState : State<GameStateMachine.Context>
    {
        private readonly UIRoot _uiRoot;
        private readonly World _world;
        private readonly RunStatisticsService _runStatisticsService;
        private readonly EnhancementService _enhancementService;

        private readonly Filter _crowdMembers;
        private readonly LevelService _levelService;

        public GameplayState(AllServices services)
        {
            _world = World.Default;
            _runStatisticsService = services.Get<RunStatisticsService>();
            _enhancementService = services.Get<EnhancementService>();
            _levelService = services.Get<LevelService>();
            _uiRoot = services.Get<UIRoot>();

            _crowdMembers = _world!.Filter
                .With<CrowdMemberTag>()
                .With<EntityViewLink>()
                .Build();
        }

        public override void Update(GameStateMachine.Context ctx)
        {
            _levelService.LevelTime += Time.deltaTime;
        }

        public override UniTask Enter(GameStateMachine.Context ctx)
        {
#if GAMEPUSH_ENABLED
            GP_Game.GameplayStart();
#endif            
            
            _levelService.ResetLevelTime();
            _uiRoot.GameWindow.MenuContent.Hide();
            _uiRoot.GameWindow.SetGameState(GameState.GAMEPLAY);
            _runStatisticsService.StartNewRun();

            return UniTask.CompletedTask;
        }

        public override UniTask EnterOnEndOfFrame(GameStateMachine.Context ctx)
        {
            _world.Commit();

            foreach (Entity member in _crowdMembers)
            {
                ref readonly EntityViewLink viewLink = ref member.GetComponent<EntityViewLink>();
                CrowdMemberView view = (CrowdMemberView)viewLink.View;

                view.StartJump();
                view.StopIdle();
            }

            return UniTask.CompletedTask;
        }

        public override UniTask Exit(GameStateMachine.Context ctx)
        {
#if GAMEPUSH_ENABLED
            GP_Game.GameplayStop();
#endif            
            _levelService.ResetLevelTime();
            _runStatisticsService.EndRun();
            _enhancementService.ResetEffects();
            
            return UniTask.CompletedTask;
        }
    }
}