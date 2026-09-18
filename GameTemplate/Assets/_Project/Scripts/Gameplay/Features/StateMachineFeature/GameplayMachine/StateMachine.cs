using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.GameplayMachine
{
    public abstract partial class StateMachine<TEvent, TContext> : StateMachineBase<TContext>
        where TEvent : IComparable where TContext : class, IStateContext
    {
        private readonly Dictionary<Type, Dictionary<TEvent, Type>> _transitions =
            new Dictionary<Type, Dictionary<TEvent, Type>>();

        private readonly Queue<StateTransition> _pendingTransitions = new Queue<StateTransition>();
        private bool _isTransitioning;

        [ShowInInspector]
        private CancellationTokenSource _rootTokenSource;

        protected StateMachine(TContext context) : base(context)
        {
            _rootTokenSource = new CancellationTokenSource();
            context.Token = _rootTokenSource.Token;
        }

        private struct StateTransition
        {
            public readonly Type TargetState;
            public readonly TContext Context;

            public StateTransition(Type targetState, TContext context)
            {
                TargetState = targetState;
                Context = context;
            }
        }

        public override void Dispose()
        {
            CancelRootToken();

            base.Dispose();
        }

        protected void CancelRootToken()
        {
            if (_rootTokenSource != null && !_rootTokenSource.IsCancellationRequested)
            {
                _rootTokenSource.Cancel();
                _rootTokenSource.Dispose();
                _rootTokenSource = null;
            }
        }

        public override void SwitchTo(Type newState, TContext context = null)
        {
            _pendingTransitions.Enqueue(new StateTransition(newState, GetValidContext(context)));

            if (!_isTransitioning)
                ProcessTransitionQueue().Forget();
        }

        private async UniTask ProcessTransitionQueue()
        {
            if (_isTransitioning)
                return;

            _isTransitioning = true;

            while (_pendingTransitions.Count > 0)
            {
                StateTransition transition = _pendingTransitions.Dequeue();
                await ExecuteTransition(transition);
            }

            _isTransitioning = false;
        }

        public CancellationToken CreateLinkedToken()
        {
            if (_rootTokenSource == null || _rootTokenSource.IsCancellationRequested)
                _rootTokenSource = new CancellationTokenSource();

            return CancellationTokenSource.CreateLinkedTokenSource(_rootTokenSource.Token).Token;
        }

        private async UniTask ExecuteTransition(StateTransition transition)
        {
            if (CurrentState != null)
            {
                await CurrentState.Exit(transition.Context);
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: transition.Context.Token);
                CurrentState.ExitOnEndOfFrame(transition.Context);
                await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: transition.Context.Token);
            }

            PreviousState = CurrentState;
            CurrentState = States[transition.TargetState];

            await CurrentState.Enter(transition.Context);
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, cancellationToken: transition.Context.Token);
            await CurrentState.EnterOnEndOfFrame(transition.Context);
            await UniTask.NextFrame(PlayerLoopTiming.Update, cancellationToken: transition.Context.Token);
        }

        public void Fire(TEvent eventType, TContext context = null)
        {
            if (CurrentState != null)
            {
                if (!_transitions.TryGetValue(CurrentState.GetType(), out Dictionary<TEvent, Type> transitions))
                {
#if DEBUG
                    Debug.LogError($"Failed to get current state: {CurrentState}");
#endif

                    return;
                }

                if (!transitions.TryGetValue(eventType, out Type nextStateType))
                {
#if DEBUG
                    Debug.LogWarning($"No transition found for trigger '{eventType}' in state '{CurrentState}'");
#endif

                    return;
                }

                if (nextStateType == CurrentState.GetType())
                {
#if DEBUG
                    Debug.LogWarning($"Transition to the same state '{CurrentState}' for trigger '{eventType}'");
#endif

                    return;
                }

                PreviousState = CurrentState;
                SwitchTo(nextStateType, context);
            }
        }

        public void Enter<TState>() where TState : IState
        {
            Type nextStateType = typeof(TState);
            PreviousState = CurrentState;
            SwitchTo(nextStateType, Ctx);
        }

#if DEBUG
        protected void Log()
        {
            foreach (KeyValuePair<Type, Dictionary<TEvent, Type>> transition in _transitions)
            {
                foreach (KeyValuePair<TEvent, Type> state in transition.Value)
                {
                    Debug.Log($"State: {transition.Key} -> Event: {state.Key} -> Next State: {state.Value}");
                }
            }
        }
#endif
    }
}