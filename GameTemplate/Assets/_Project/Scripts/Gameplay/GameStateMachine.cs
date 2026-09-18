using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.GameplayMachine;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    [Serializable]
    public class GameStateMachine : StateMachine<GameplayEvent, GameStateMachine.Context>, IService
    {
        private readonly World _world;

        private GameStateMachine(Context context) : base(context)
        {
            _world = World.Default;
        }

        public static GameStateMachine Configure(
            AllServices services,
            Context context
        )
        {
            GameStateMachine machine = new GameStateMachine(context);

            services.Register(machine);

            Builder builder = new Builder(machine);

            GameStateMachine gameStateMachine = builder
                .AddState(new BootstrapState(services))
                .AddState(new MenuState(services))
                .AddState(new GameplayState(services))
                .AddState(new GameEndingState(services))
                .AddState(new DefeatState(services))
                .AddState(new VictoryState(services))
                //
                .From<BootstrapState>()
                .On(GameplayEvent.ENTER_MENU)
                .To<MenuState>()
                //
                .From<MenuState>()
                .On(GameplayEvent.ENTER_GAMEPLAY)
                .To<GameplayState>()
                //
                .From<GameplayState>()
                .On(GameplayEvent.ENTER_GAME_ENDING)
                .To<GameEndingState>()
                //
                .From<GameEndingState>()
                .On(GameplayEvent.VICTORY)
                .To<VictoryState>()
                //
                .From<GameEndingState>()
                .On(GameplayEvent.DEFEAT)
                .To<DefeatState>()
                //
                .From<VictoryState>()
                .On(GameplayEvent.ENTER_MENU)
                .To<MenuState>()
                //
                .From<DefeatState>()
                .On(GameplayEvent.ENTER_MENU)
                .To<MenuState>()
                //
                .Build();

#if DEBUG
            machine.Log();
#endif

            return gameStateMachine;
        }

        public override void Update()
        {
            base.Update();

            _world.Update(Time.deltaTime);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            _world.LateUpdate(Time.deltaTime);
            _world.CleanupUpdate(Time.deltaTime);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            _world.FixedUpdate(Time.fixedDeltaTime);
        }

        public override void Dispose() =>
            CancelRootToken();

        private class Builder : Builder<GameStateMachine>
        {
            public Builder(GameStateMachine machine) : base(machine)
            {
            }
        }

        [Serializable]
        public record Context : IStateContext
        {
            public CancellationToken Token { get; set; }

            [ShowInInspector]
            public GameEndingState.Reason GameEndReason { get; set; }
        }
    }
}