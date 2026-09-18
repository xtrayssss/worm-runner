using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PrepareStatModifiersSystem : ISystem
    {
        public World World { get; set; }

        private Filter _modifiers;

        public void OnAwake()
        {
            _modifiers = World.Filter
                .With<StatModifierTag>()
                .With<ModifierValue>()
                .With<AddStatModifierRequest>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity modifier in _modifiers)
            {
                ref AddStatModifierRequest addStatModifierRequest = ref modifier.GetComponent<AddStatModifierRequest>();
                ref ModifierValue modifierValue = ref modifier.GetComponent<ModifierValue>();
                addStatModifierRequest.Modifier.Value = modifierValue.Value;
            }
        }

        public void Dispose()
        {
        }
    }
}