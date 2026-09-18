using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.GameplayMachine
{
    public abstract partial class StateMachine<TEvent, TContext> where TEvent : IComparable where TContext : class, IStateContext
    {
        public class Builder<TMachine> : Builder<TMachine, Builder<TMachine>>
            where TMachine : StateMachine<TEvent, TContext>
        {
            public Builder(TMachine machine) : base(machine)
            {
            }

            public StateConfiguration<TMachine> From<TState>() where TState : IState<TContext>
            {
                Machine._transitions.TryGetValue(typeof(TState), out Dictionary<TEvent, Type> transitions);
                transitions ??= new Dictionary<TEvent, Type>();
                return new StateConfiguration<TMachine>(this, typeof(TState), transitions);
            }
        }
    }
}