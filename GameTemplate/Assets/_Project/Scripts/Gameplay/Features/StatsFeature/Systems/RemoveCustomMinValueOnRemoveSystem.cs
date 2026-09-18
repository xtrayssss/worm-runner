using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RemoveCustomMinValueOnRemoveSystem : ISystem
    {
        public World World { get; set; }

        private Filter _modifiers;

        public void OnAwake()
        {
            _modifiers = World.Filter
                .With<StatModifierTag>()
                .With<CustomMinValueOnRemove>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity modifier in _modifiers)
                modifier.RemoveComponent<CustomMinValueOnRemove>();
        }

        public void Dispose()
        {
        }
    }
}