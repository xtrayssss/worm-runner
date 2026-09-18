using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UnapplyFreezeStatusSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statuses;

        private readonly StatsService _statsService;

        public UnapplyFreezeStatusSystem(StatsService statsService) =>
            _statsService = statsService;

        public void OnAwake()
        {
            _statuses = World.Filter
                .With<FreezeStatusTag>()
                .With<StatusRemovedEvent>()
                .With<StatusProducedModifiers>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _statuses)
            {
                ref readonly StatusProducedModifiers producedModifiers =
                    ref status.GetComponent<StatusProducedModifiers>();

                foreach (Entity modifier in producedModifiers.Value)
                    _statsService.RemoveModifier(modifier);
            }
        }

        public void Dispose()
        {
        }
    }
}