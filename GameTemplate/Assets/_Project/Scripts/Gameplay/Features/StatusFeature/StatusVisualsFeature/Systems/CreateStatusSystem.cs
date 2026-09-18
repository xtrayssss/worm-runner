using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.StatusVisualsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CreateStatusSystem : ISystem
    {
        public World World { get; set; }

        private Filter _activeStatuses;
        private readonly StatusFactory _statusFactory;
        private Request<CreateStatusRequest> _createStatusRequests;

        public CreateStatusSystem(StatusFactory statusFactory)
        {
            _statusFactory = statusFactory;
        }

        public void OnAwake()
        {
            _createStatusRequests = World.GetRequest<CreateStatusRequest>();

            _activeStatuses = World.Filter
                .With<StatusTag>()
                .With<StatusTypeLink>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (CreateStatusRequest createRequest in _createStatusRequests.Consume())
            {
                Entity existingStatus = FindExistingStatusOnTarget(createRequest.Setup, createRequest.Target);

                if (!existingStatus.IsNullOrDisposed() && createRequest.Setup.UseStacking)
                {
                    ref StatusStackCount stackCount = ref existingStatus.GetComponent<StatusStackCount>();
                    stackCount.Value = Mathf.Min(stackCount.Value + 1, createRequest.Setup.MaxStacks);

                    int totalStacks = CountTotalStacksForTarget(createRequest.Setup.StatusTypeId, createRequest.Target);

                    if (!existingStatus.Has<StatusAppliedEvent>())
                        existingStatus.AddComponent<StatusAppliedEvent>().StackCount = totalStacks;
                    else
                        existingStatus.GetComponent<StatusAppliedEvent>().StackCount = totalStacks;

                    ref StatusTimeLeft timeLeft = ref existingStatus.GetComponent<StatusTimeLeft>();
                    timeLeft.Value = createRequest.Setup.Duration;
                }
                else
                {
                    Entity createdStatus = _statusFactory.CreateStatus(
                        setup: createRequest.Setup,
                        producer: createRequest.Producer,
                        target: createRequest.Target);

                    int totalStacks = CountTotalStacksForTarget(createRequest.Setup.StatusTypeId, createRequest.Target);
                    createdStatus.AddComponent<StatusAppliedEvent>().StackCount = totalStacks + 1;
                    
                    if (!createRequest.EnhancementSource.IsNullOrDisposed())
                    {
                        StatusEnhancements.Create()
                            .ForStatus(createdStatus);
                    }
                }
            }
        }

        private Entity FindExistingStatusOnTarget(StatusSetup setup, Entity targetId)
        {
            foreach (Entity status in _activeStatuses)
            {
                ref readonly TargetId statusTarget = ref status.GetComponent<TargetId>();

                if (statusTarget.Value != targetId)
                    continue;

                ref readonly StatusTypeLink statusTypeLink = ref status.GetComponent<StatusTypeLink>();

                if (statusTypeLink.Value == setup.StatusTypeId)
                    return status;
            }

            return default;
        }

        private int CountTotalStacksForTarget(StatusTypeId statusTypeId, Entity target)
        {
            int totalStacks = 0;

            foreach (Entity status in _activeStatuses)
            {
                ref readonly TargetId statusTarget = ref status.GetComponent<TargetId>();

                if (statusTarget.Value != target)
                    continue;

                ref readonly StatusTypeLink typeLink = ref status.GetComponent<StatusTypeLink>();

                if (typeLink.Value != statusTypeId)
                    continue;

                totalStacks += status.GetComponent<StatusStackCount>().Value;
            }

            return totalStacks;
        }

        public void Dispose()
        {
        }
    }
}