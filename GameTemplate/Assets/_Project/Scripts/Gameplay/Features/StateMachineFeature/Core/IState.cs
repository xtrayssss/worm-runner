using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public interface IState
    {
    }

    public interface IState<in TContext> : IState where TContext : IStateContext
    {
        public UniTask Enter(TContext ctx);
        public UniTask Exit(TContext ctx);
        public void Update(TContext ctx);
        public void Dispose();
        public void FixedUpdate(TContext ctx);
        public void LateUpdate(TContext ctx);
        public UniTask EnterOnEndOfFrame(TContext ctx);
        public void ExitOnEndOfFrame(TContext ctx);
    }
}