using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.GameplayMachine
{
    public partial class StateMachine<TEvent, TContext> where TEvent : IComparable where TContext : class, IStateContext
    {
        public class StateConfiguration<TMachine> where TMachine : StateMachine<TEvent, TContext>
        {
            private readonly Builder<TMachine> _builder;
            private TEvent _lastEvent;
            private readonly Type _stateType;
            private readonly Dictionary<TEvent, Type> _transitions;

            public StateConfiguration(
                Builder<TMachine> builder,
                Type stateType,
                Dictionary<TEvent, Type> transitions)
            {
                _builder = builder;
                _stateType = stateType;
                _transitions = transitions;
            }

            public StateConfiguration<TMachine> On(TEvent eventType)
            {
                _transitions.Add(eventType, null);
                _lastEvent = eventType;
                return this;
            }

            public StateConfiguration<TMachine> To<TState>() where TState : IState<TContext>
            {
                _transitions[_lastEvent] = typeof(TState);
                _builder.Machine._transitions[_stateType] = _transitions;
                return this;
            }
            public TMachine Build() => 
                _builder.Build();

            public Builder<TMachine> InitialState<TState>() where TState : IState<TContext> =>
                _builder.InitialState<TState>();

            public StateConfiguration<TMachine> From<TState>() where TState : IState<TContext> =>
                _builder.From<TState>();
        }
    }
}