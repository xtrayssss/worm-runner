using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.EntityMachine
{
    [Serializable]
    public partial class EntityStateMachine<TContext> : IUpdatableStateMachine
        where TContext : class, IEntityStateContext<TContext>, IStateContext
    {
        private readonly List<Transition> _anyTransitions = new List<Transition>();
        private readonly Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();

        [ShowInInspector]
        private readonly Dictionary<Entity, TContext> _entityContexts = new Dictionary<Entity, TContext>();

        [ShowInInspector]
        protected readonly Dictionary<Type, IState<TContext>> States = new Dictionary<Type, IState<TContext>>();

        private Type _initialState;

        public void RegisterEntity(Entity entity, TContext context)
        {
            context.Entity = entity;
            context.CurrentState = States[_initialState];
            _entityContexts[entity] = context;
            context.CurrentState.Enter(context);

            CancellationTokenSource rootTokenSource = new CancellationTokenSource();
            context.RootTokenSource = rootTokenSource;
            context.Token = rootTokenSource.Token;

            entity.AddComponent<EntityBrain>().Value = this as EntityStateMachine<EntityStateContext>;
        }

        public void UnregisterEntity(Entity entity) =>
            _entityContexts.Remove(entity);

        public void Update()
        {
            foreach ((_, TContext context) in _entityContexts)
            {
                Transition transition = GetTransition(context);

                if (transition != null)
                {
                    context.CurrentState?.Exit(context);
                    context.PreviousState = context.CurrentState;
                    context.CurrentState = transition.ToState;
                    context.CurrentState.Enter(context);
                }

                context.CurrentState?.Update(context);
            }
        }

        private Transition GetTransition(TContext context)
        {
            foreach (Transition transition in _anyTransitions)
                if (transition.Condition(context.Entity) && transition.ToState != context.CurrentState)
                    return transition;

            if (context.CurrentState != null &&
                _transitions.TryGetValue(context.CurrentState.GetType(), out List<Transition> transitions))
            {
                foreach (var transition in transitions)
                    if (transition.Condition(context.Entity))
                        return transition;
            }

            return null;
        }

        public void AddTransition<TFrom, TTo>(Func<Entity, bool> condition)
            where TFrom : IState<TContext>
            where TTo : IState<TContext>
        {
            Type fromType = typeof(TFrom);
            Type toType = typeof(TTo);

            if (!_transitions.TryGetValue(fromType, out List<Transition> transitions))
            {
                transitions = new List<Transition>();
                _transitions[fromType] = transitions;
            }

            transitions.Add(new Transition(States[toType], condition));
        }

        public void AddAnyTransition<TTo>(Func<Entity, bool> condition) where TTo : IState<TContext>
        {
            _anyTransitions.Add(new Transition(toState: States[typeof(TTo)], condition));
        }

        public class Transition
        {
            public IState<TContext> ToState { get; }
            public Func<Entity, bool> Condition { get; }

            public Transition(IState<TContext> toState, Func<Entity, bool> condition)
            {
                ToState = toState;
                Condition = condition;
            }
        }

        public void Cleanup() =>
            _entityContexts.Clear();
    }
}