using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RequestRemoveModifierSystem : ISystem
    {
        public World World { get; set; }

        private Filter _deadModifiers;

        public void OnAwake()
        {
            _deadModifiers = World.Filter
                .With<StatModifierTag>()
                .With<DeathModifierMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity modifier in _deadModifiers)
            {
                ref readonly ModifierId modifierId = ref modifier.GetComponent<ModifierId>();

                modifier.AddComponent<RemoveStatModifierRequest>() = new RemoveStatModifierRequest
                {
                    StatId = modifierId.StatId,
                    Id = modifierId.Id,
                    CustomMinValue = modifier.Has<CustomMinValueOnRemove>()
                        ? modifier.GetComponent<CustomMinValueOnRemove>().Value
                        : null
                };
            }
        }

        public void Dispose()
        {
        }
    }
}