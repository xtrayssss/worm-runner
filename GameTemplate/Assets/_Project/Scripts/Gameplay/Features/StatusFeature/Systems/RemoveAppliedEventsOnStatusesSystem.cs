using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RemoveAppliedEventsOnStatusesSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statuses;

        public void OnAwake()
        {
            _statuses = World.Filter
                .With<StatusTag>()
                .With<StatusAppliedEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _statuses) 
                status.RemoveComponent<StatusAppliedEvent>();
        }

        public void Dispose()
        {
        }
    }
}