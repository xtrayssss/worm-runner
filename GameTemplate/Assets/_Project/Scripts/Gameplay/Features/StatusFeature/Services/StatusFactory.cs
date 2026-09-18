using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Services
{
    public sealed class StatusFactory : IService
    {
        public Entity CreateStatus(StatusSetup setup, Entity producer, Entity target)
        {
            return setup.StatusTypeId switch
            {
                StatusTypeId.POISON => ConstructPoisonStatus(setup, producer, target),
                StatusTypeId.FREEZE => ConstructFreezeStatus(setup, producer, target),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private Entity ConstructPoisonStatus(StatusSetup setup, Entity producer, Entity target)
        {
            Entity statusEntity = InitializeBaseStatus(setup, producer, target);
            statusEntity.AddComponent<PoisonStatusTag>();
            statusEntity.AddComponent<StatusTimeLeft>().Value = setup.Duration;
            statusEntity.AddComponent<StatusPeriod>().Value = setup.Period;
            statusEntity.AddComponent<StatusTimeSinceLastTick>().Value = 0;
#if DEBUG
            statusEntity.AddComponent<EntityName>().Value = "Poison";
#endif

            statusEntity.AddComponent<Stats>() = new StatsBuilder().SetDamage(setup.ModifierSetup.ModifierValue).Build();

            return statusEntity;
        }

        private Entity ConstructFreezeStatus(StatusSetup setup, Entity producer, Entity target)
        {
            Entity statusEntity = InitializeBaseStatus(setup, producer, target);
            statusEntity.AddComponent<FreezeStatusTag>();
            statusEntity.AddComponent<StatusTimeLeft>().Value = setup.Duration;
#if DEBUG
            statusEntity.AddComponent<EntityName>().Value = "Freeze";
#endif

            return statusEntity;
        }

        private Entity InitializeBaseStatus(StatusSetup setup, Entity producer, Entity target)
        {
            Entity statusEntity = World.Default.CreateEntity();

            statusEntity.AddComponent<StatusTag>();

            statusEntity.AddComponent<StatusTypeLink>().Value = setup.StatusTypeId;

            statusEntity.AddComponent<Producer>().Value = producer;
            statusEntity.AddComponent<TargetId>().Value = target;

            statusEntity.AddComponent<StatModifierSetupReference>().Value = setup.ModifierSetup;
            statusEntity.AddComponent<StatusAppliedMarker>();
            statusEntity.AddComponent<StatusProducedModifiers>().Value = new List<Entity>();
            statusEntity.AddComponent<Position>();

            statusEntity.AddComponent<StatusStackCount>().Value = 1;

            if (setup.UseStacking)
                statusEntity.AddComponent<StackableStatusMarker>();

            statusEntity.AddComponent<StatusCreatedEvent>();

            return statusEntity;
        }
    }
}