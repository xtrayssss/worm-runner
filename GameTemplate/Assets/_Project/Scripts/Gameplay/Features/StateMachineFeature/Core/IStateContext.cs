using System.Threading;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public interface IStateContext
    { 
        public CancellationToken Token { get; set; }
    }
}