namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public abstract partial class StateMachineBase<TContext>
    {
        public class Builder<TMachine, TBuilder> where TMachine : StateMachineBase<TContext>
            where TBuilder : Builder<TMachine, TBuilder>
        {
            public readonly TMachine Machine;

            public Builder(TMachine machine) =>
                Machine = machine;

            public virtual TBuilder AddState(IState<TContext> state)
            {
                Machine.States.Add(state.GetType(), state);
                return (TBuilder)this;
            }

            public virtual TBuilder InitialState<TState>() where TState : IState<TContext>
            {
                Machine.SwitchTo(typeof(TState));
                return (TBuilder)this;
            }
            
            public TBuilder InitialState<TState>(TContext context) where TState : IState<TContext>
            {
                Machine.SwitchTo(typeof(TState), context);
                return (TBuilder)this;
            }

            public TBuilder DefineSubMachine<TSubMachine>() where TSubMachine : IStateMachine
            {
                Machine.DefineSubMachine<TSubMachine>(null);

                return (TBuilder)this;
            }

            public virtual TMachine Build() =>
                Machine;
        }
    }
}