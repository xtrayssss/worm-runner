using _Project.Scripts.Gameplay.Features.StateMachineFeature.Core;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StateMachineFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpdateStateMachineSystem : ISystem
    {
        public World World { get; set; }

        private Filter _agents;

        public void OnAwake() =>
            _agents = World.Filter.With<StateMachine>().Build();

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity brain in _agents)
                brain.GetComponent<StateMachine>().Value.Update();
        }

        public void Dispose()
        {
        }
    }
}