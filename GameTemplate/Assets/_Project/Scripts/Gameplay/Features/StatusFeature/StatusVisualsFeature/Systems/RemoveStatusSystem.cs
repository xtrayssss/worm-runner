using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RemoveStatusSystem : ISystem
    {
        public World World { get; set; }

        private Filter _dyingStatuses;

        public void OnAwake()
        {
            _dyingStatuses = World.Filter
                .With<StatusTag>()
                .With<StatusTypeLink>()
                .With<StatusDeathMarker>()
                .With<TargetId>()
                .Without<StatusRemovedEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _dyingStatuses)
            {
                ref readonly StatusTypeLink type = ref status.GetComponent<StatusTypeLink>();
                ref readonly TargetId targetId = ref status.GetComponent<TargetId>();

                int remainingStacks = CalculateRemainingStacks(type.Value, targetId.Value, status);

                status.AddComponent<StatusRemovedEvent>().RemainingStacks = remainingStacks;
            }
        }

        private int CalculateRemainingStacks(StatusTypeId statusTypeId, Entity target, Entity excludeStatus)
        {
            int remainingStacks = 0;

            Filter statusFilter = World.Filter
                .With<StatusTag>()
                .With<StatusTypeLink>()
                .With<TargetId>()
                .Without<StatusDeathMarker>()
                .Build();

            foreach (Entity status in statusFilter)
            {
                if (status == excludeStatus)
                    continue;

                ref readonly TargetId statusTarget = ref status.GetComponent<TargetId>();
                if (statusTarget.Value != target)
                    continue;

                ref readonly StatusTypeLink typeLink = ref status.GetComponent<StatusTypeLink>();
                if (typeLink.Value != statusTypeId)
                    continue;

                remainingStacks += status.GetComponent<StatusStackCount>().Value;
            }

            return remainingStacks;
        }

        public void Dispose()
        {
        }
    }
}