using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Properties;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public abstract class StateMachineBase
    {
    }

    [Serializable]
    public abstract partial class StateMachineBase<TContext> : StateMachineBase, IStateMachine
        where TContext : class, IStateContext
    {
        [ShowInInspector]
        protected readonly Dictionary<Type, IState<TContext>> States = new Dictionary<Type, IState<TContext>>();

        [ShowInInspector]
        protected readonly Dictionary<Type, IStateMachine> SubMachines =
            new Dictionary<Type, IStateMachine>();

        [ShowInInspector]
        public IState<TContext> CurrentState { get; protected set; }

        [ShowInInspector]
        protected IState<TContext> PreviousState;

        [ShowInInspector]
        public TContext Ctx { get; protected set; }

        protected StateMachineBase(TContext context)
        {
            Ctx = context;
        }

        public virtual void Update()
        {
            foreach (IStateMachine subMachine in SubMachines.Values)
                subMachine?.Update();

            CurrentState?.Update(Ctx);
        }

        public virtual void LateUpdate()
        {
            foreach (IStateMachine subMachine in SubMachines.Values)
                subMachine?.LateUpdate();

            CurrentState?.LateUpdate(Ctx);
        }

        public virtual void FixedUpdate()
        {
            foreach (IStateMachine subMachine in SubMachines.Values)
                subMachine?.FixedUpdate();

            CurrentState?.FixedUpdate(Ctx);
        }

        public virtual void Dispose()
        {
            foreach (IStateMachine subMachine in SubMachines.Values)
                subMachine?.Dispose();

            CurrentState?.Exit(Ctx);
            CurrentState?.Dispose();
        }

        public void GoBack()
        {
            if (PreviousState != null)
            {
                SwitchTo(PreviousState.GetType());
                PreviousState = null;
            }
        }

        public bool IsInState(Type stateType) =>
            CurrentState != null && CurrentState.GetType() == stateType;

        public void AddSubMachine(IStateMachine machine) =>
            SubMachines[machine.GetType()] = machine;

        public void RemoveSubMachine<TMachine>() where TMachine : StateMachineBase =>
            SubMachines.Remove(typeof(TMachine));

        private void DefineSubMachine<TSubMachine>(StateMachineBase<TContext> machine)
            where TSubMachine : IStateMachine =>
            SubMachines.Add(typeof(TSubMachine), machine);

        public TState GetState<TState>() where TState : IState<TContext> =>
            (TState)States[typeof(TState)];

        private async UniTask ExecuteEnter(IState<TContext> state, TContext context = null) =>
            await state.Enter(GetValidContext(context));

        private void ExecuteExit(IState<TContext> state, TContext context = null) =>
            state?.Exit(GetValidContext(context));

        public virtual void SwitchTo(Type newState, TContext context = null)
        {
            ExecuteExit(CurrentState, GetValidContext(context));
            CurrentState = States[newState];
            ExecuteEnter(CurrentState, GetValidContext(context)).Forget();
        }

        protected TContext GetValidContext(TContext context) =>
            context ?? Ctx;

        public TMachine GetSubMachine<TMachine>() where TMachine : StateMachineBase =>
            SubMachines.GetValueOrDefault(typeof(TMachine), null) as TMachine;
    }
}