using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Services
{
    public sealed class StatusApplier : IService
    {
        public void TryApplyStatus(StatusSetup setup, Entity producer, Entity target,
            Entity enhancementSource = default)
        {
            if (!setup.AutoApply)
                return;

            CreateStatusRequest(setup, producer, target, enhancementSource);
        }

        public void ApplyStatus(StatusSetup setup, Entity producer, Entity target, Entity enhancementSource = default)
        {
            CreateStatusRequest(setup, producer, target, enhancementSource);
        }

        private void CreateStatusRequest(StatusSetup setup, Entity producer, Entity target,
            Entity enhancementSource = default)
        {
            World.Default
                .GetRequest<CreateStatusRequest>()
                .Publish(new CreateStatusRequest
                {
                    Setup = setup,
                    Producer = producer,
                    Target = target,
                    EnhancementSource = enhancementSource
                }, allowNextFrame: true);
        }

        public StatusSetup FindStatusSetupInAbility(
            Entity ability,
            StatusTypeId statusTypeId,
            Predicate<StatusSetup> additionalCondition = null)
        {
            if (!ability.Has<StatusSetups>())
                return null;

            ref readonly StatusSetups statusSetups = ref ability.GetComponent<StatusSetups>();

            foreach (StatusSetup status in statusSetups.Value)
            {
                if (status.StatusTypeId != statusTypeId)
                    continue;

                if (additionalCondition != null && !additionalCondition(status))
                    continue;

                return status;
            }

            return null;
        }
    }
}