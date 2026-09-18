using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public class State<TContext> : IState<TContext> where TContext : IStateContext
    {
        public virtual UniTask Enter(TContext ctx) => 
            UniTask.CompletedTask;

        public virtual UniTask Exit(TContext ctx) => 
            UniTask.CompletedTask;

        public virtual void Update(TContext ctx)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual void FixedUpdate(TContext ctx)
        {
        }

        public virtual void LateUpdate(TContext ctx)
        {
        }

        public virtual UniTask EnterOnEndOfFrame(TContext ctx) => 
            UniTask.CompletedTask;

        public virtual void ExitOnEndOfFrame(TContext ctx)
        {
        }
    }
}