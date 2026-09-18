using _Project.Scripts.Gameplay.Extensions;
using _Project.Scripts.Gameplay.Features.AdvertisementFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Systems;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using Cysharp.Threading.Tasks;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    public sealed class GameEndingState : State<GameStateMachine.Context>
    {
        public enum Reason
        {
            UNKNOWN = 0,
            DETECTION = 1,
            BONUS_CHEST = 3,
            ALL_ENEMIES_DEFEATED = 4,
            FINISH_REACHED = 5,
            CROWD_EXTINCT = 6
        }

        private readonly LevelService _levelService;
        private readonly GameStateMachine _gameStateMachine;
        private readonly Filter _crowds;
        private readonly CrowdRemoveMembersSystem _crowdRemoveMembersSystem;
        private readonly World _world;
        private readonly AdvertisementService _advertisementService;

        public GameEndingState(AllServices services)
        {
            _levelService = services.Get<LevelService>();
            _gameStateMachine = services.Get<GameStateMachine>();
            _crowdRemoveMembersSystem = services.Get<CrowdRemoveMembersSystem>();
            _advertisementService = services.Get<AdvertisementService>();
            _world = World.Default;

            _crowds = _world!.Filter
                .With<CrowdTag>()
                .Without<StopMovementMarker>()
                .Build();
        }

        public override async UniTask EnterOnEndOfFrame(GameStateMachine.Context ctx)
        {
            Reason reason = ctx.GameEndReason;

            StopCrowdMovement();

            await HandleGameEndReason(reason);

            bool isVictory = DetermineVictory(reason);

            _world.Commit();

            foreach (Entity entity in World.Default.ClearWorld())
            {
                if (entity.Has<CrowdMemberTag>())
                {
                    EntityViewLink viewLink = entity.GetComponent<EntityViewLink>();
                    CrowdMemberView view = (CrowdMemberView)viewLink.View;
                    view.StopJump();
                }
            }
            
            await _advertisementService.ShowInterstitialAsync();

            _gameStateMachine.Fire(isVictory
                ? GameplayEvent.VICTORY
                : GameplayEvent.DEFEAT);
        }

        private void StopCrowdMovement()
        {
            foreach (Entity crowd in _crowds)
                crowd.AddComponent<StopMovementMarker>();
        }

        private async UniTask HandleGameEndReason(Reason reason)
        {
            switch (reason)
            {
                case Reason.DETECTION:
                    await _crowdRemoveMembersSystem.RemoveAllMilitaryMembersWithDissolveAsync();
                    break;

                case Reason.CROWD_EXTINCT:
                case Reason.BONUS_CHEST:
                case Reason.FINISH_REACHED:
                case Reason.ALL_ENEMIES_DEFEATED:
                    break;
            }
        }

        private bool DetermineVictory(Reason reason)
        {
            return reason switch
            {
                Reason.BONUS_CHEST or Reason.FINISH_REACHED or Reason.CROWD_EXTINCT =>
                    _levelService.IsLevelCompleted(_levelService.CurrentLevelIndex),

                Reason.ALL_ENEMIES_DEFEATED => true,

                Reason.DETECTION => false,

                _ => false
            };
        }
    }
}