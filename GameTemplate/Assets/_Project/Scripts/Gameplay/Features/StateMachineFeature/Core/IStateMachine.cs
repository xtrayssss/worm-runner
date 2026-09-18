namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Core
{
    public interface IStateMachine
    {
        public void Update();
        public void LateUpdate();
        public void FixedUpdate();
        public void Dispose();
    }
}