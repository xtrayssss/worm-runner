using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StatusLifetimeSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statuses;

        public void OnAwake()
        {
            _statuses = World.Filter
                .With<StatusTag>()
                .With<StatusAppliedMarker>()
                .With<StatusTimeLeft>()
                .Without<StatusDeathMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _statuses)
            {
                ref StatusTimeLeft timeLeft = ref status.GetComponent<StatusTimeLeft>();

                if (timeLeft.Value >= 0)
                    timeLeft.Value -= deltaTime;
                else
                    status.AddComponent<StatusDeathMarker>();
            }
        }

        public void Dispose()
        {
        }
    }
}
