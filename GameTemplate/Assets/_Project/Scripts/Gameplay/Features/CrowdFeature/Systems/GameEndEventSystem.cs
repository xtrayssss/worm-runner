using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.LevelFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.StealthFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.CrowdFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GameEndEventSystem : ISystem
    {
        public World World { get; set; }

        private Event<CrowdExtinctEvent> _crowdExtinctEvent;
        private Event<FinishReachedEvent> _finishReachedEvent;
        private Event<BonusChestOpenedEvent> _bonusChestOpenedEvent;
        private Event<MemberDetectionEvent> _detectionEvents;
        private Event<AllEnemiesDefeatedEvent> _allEnemiesDefeatedEvent;

        private readonly GameStateMachine _gameStateMachine;
        private readonly LevelService _levelService;

        public GameEndEventSystem(
            GameStateMachine gameStateMachine,
            LevelService levelService)
        {
            _gameStateMachine = gameStateMachine;
            _levelService = levelService;
        }

        public void OnAwake()
        {
            _crowdExtinctEvent = World.GetEvent<CrowdExtinctEvent>();
            _bonusChestOpenedEvent = World.GetEvent<BonusChestOpenedEvent>();
            _detectionEvents = World.GetEvent<MemberDetectionEvent>();
            _finishReachedEvent = World.GetEvent<FinishReachedEvent>();
            _allEnemiesDefeatedEvent = World.GetEvent<AllEnemiesDefeatedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_gameStateMachine.CurrentState is not GameplayState)
                return;

            foreach (BonusChestOpenedEvent _ in _bonusChestOpenedEvent.publishedChanges)
            {
                TriggerGameEnd(GameEndingState.Reason.BONUS_CHEST);
                return;
            }

            foreach (FinishReachedEvent _ in _finishReachedEvent.publishedChanges)
            {
                switch (_levelService.CurrentLevel.LevelMode)
                {
                    case LevelMode.STEALTH:
                    {
                        TriggerGameEnd(GameEndingState.Reason.FINISH_REACHED);
                        return;
                    }
                }

                return;
            }

            foreach (AllEnemiesDefeatedEvent _ in _allEnemiesDefeatedEvent.publishedChanges)
            {
                TriggerGameEnd(GameEndingState.Reason.ALL_ENEMIES_DEFEATED);
                return;
            }

            foreach (CrowdExtinctEvent _ in _crowdExtinctEvent.publishedChanges)
            {
                TriggerGameEnd(GameEndingState.Reason.CROWD_EXTINCT);
                return;
            }

            foreach (MemberDetectionEvent _ in _detectionEvents.publishedChanges)
            {
                TriggerGameEnd(GameEndingState.Reason.DETECTION);
                return;
            }
        }

        private void TriggerGameEnd(GameEndingState.Reason reason)
        {
            _gameStateMachine.Ctx.GameEndReason = reason;
            _gameStateMachine.Fire(GameplayEvent.ENTER_GAME_ENDING);
        }

        public void Dispose()
        {
        }
    }
}