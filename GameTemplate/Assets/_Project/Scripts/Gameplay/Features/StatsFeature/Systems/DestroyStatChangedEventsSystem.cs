using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DestroyStatChangedEventsSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statChanges;

        public void OnAwake()
        {
            _statChanges = World.Filter
                .With<StatChangedEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity statChangeEvent in _statChanges) 
                World.RemoveEntity(statChangeEvent);
        }

        public void Dispose()
        {
        }
    }
}