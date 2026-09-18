using System;
using System.Threading;
using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.EntityMachine
{
    [Serializable]
    public class EntityStateContext : IEntityStateContext<EntityStateContext>, IStateContext
    {
        [ShowInInspector] public Entity Entity { get; set; }
        [ShowInInspector] public IState<EntityStateContext> CurrentState { get; set; }
        [ShowInInspector] public IState<EntityStateContext> PreviousState { get; set; }
        public CancellationTokenSource RootTokenSource { get; set; }
        public CancellationToken Token { get; set; }

        public void CancelRootToken()
        {
            if (RootTokenSource != null && !RootTokenSource.IsCancellationRequested)
            {
                RootTokenSource.Cancel();
                RootTokenSource.Dispose();
                RootTokenSource = null;
            }
        }
    }
}