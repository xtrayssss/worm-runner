using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SyncStatusPositionToTargetPositionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statuses;

        public void OnAwake()
        {
            _statuses = World.Filter
                .With<StatusTag>()
                .With<Position>()
                .With<TargetId>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _statuses)
            {
                ref readonly TargetId target = ref status.GetComponent<TargetId>();
                ref Position position = ref status.GetComponent<Position>();

                if (target.Value.Has<Position>())
                {
                    ref readonly Position targetPosition = ref target.Value.GetComponent<Position>();
                    position.Value = targetPosition.Value;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}