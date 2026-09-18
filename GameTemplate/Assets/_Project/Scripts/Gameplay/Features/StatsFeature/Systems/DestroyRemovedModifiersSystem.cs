using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DestroyRemovedModifiersSystem : ISystem
    {
        public World World { get; set; }
        private Filter _deadModifiers;

        public void OnAwake()
        {
            _deadModifiers = World.Filter
                .With<StatModifierTag>()
                .With<RemovedModifierMarker>()
                .With<DeathModifierMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity modifier in _deadModifiers)
                World.RemoveEntity(modifier);
        }

        public void Dispose()
        {
        }
    }
}