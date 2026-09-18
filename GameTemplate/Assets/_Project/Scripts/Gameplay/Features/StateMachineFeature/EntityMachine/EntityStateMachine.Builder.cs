using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.EntityMachine
{
    public partial class EntityStateMachine<TContext> where TContext : class, IEntityStateContext<TContext>, IStateContext
    {
        public class EntityStateMachineBuilder<TMachine>
            where TMachine : EntityStateMachine<TContext>
        {
            private readonly TMachine _machine;
            private readonly CancellationToken _cancellationToken;
            private readonly Dictionary<Type, IState<TContext>> _states = new Dictionary<Type, IState<TContext>>();

            public EntityStateMachineBuilder(TMachine machine) => 
                _machine = machine;

            public EntityStateMachineBuilder<TMachine> AddState(IState<TContext> state)
            {
                _states.Add(state.GetType(), state);
                _machine.States.Add(state.GetType(), state);
                return this;
            }

            public EntityStateMachineBuilder<TMachine> AddTransition<TFrom, TTo>(Func<Entity, bool> condition)
                where TFrom : IState<TContext>
                where TTo : IState<TContext>
            {
                var fromType = typeof(TFrom);
                var toType = typeof(TTo);

                if (!_states.TryGetValue(fromType, out var fromState))
                    throw new InvalidOperationException($"State {fromType.Name} not added to the machine.");

                if (!_states.TryGetValue(toType, out var toState))
                    throw new InvalidOperationException($"State {toType.Name} not added to the machine.");

                if (!_machine._transitions.TryGetValue(fromType, out var transitions))
                {
                    transitions = new List<Transition>();
                    _machine._transitions[fromType] = transitions;
                }

                transitions.Add(new Transition(toState, condition));
                return this;
            }

            public EntityStateMachineBuilder<TMachine> AddAnyTransition<TTo>(Func<Entity, bool> condition)
                where TTo : IState<TContext>
            {
                if (!_states.TryGetValue(typeof(TTo), out var toState))
                    throw new InvalidOperationException($"State {typeof(TTo).Name} not added to the machine.");

                _machine._anyTransitions.Add(new Transition(toState, condition));
                return this;
            }

            public EntityStateMachineBuilder<TMachine> InitialState<TState>()
            {
                _machine._initialState = typeof(TState);

                return this;
            }
        }
    }

    public interface IEntityStateContext<TContext> where TContext : IStateContext
    {
        Entity Entity { get; set; }
        IState<TContext> CurrentState { get; set; }
        IState<TContext> PreviousState { get; set; }
        CancellationTokenSource RootTokenSource { get; set; }
    }
}