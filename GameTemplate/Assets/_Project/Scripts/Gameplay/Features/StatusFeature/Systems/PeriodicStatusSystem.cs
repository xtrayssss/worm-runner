using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PeriodicStatusSystem : ISystem
    {
        private readonly StatsService _statsService;
        public World World { get; set; }

        private Filter _statuses;

        public PeriodicStatusSystem(StatsService statsService) =>
            _statsService = statsService;

        public void OnAwake()
        {
            _statuses = World.Filter
                .With<StatusTag>()
                .With<StatusPeriod>()
                .With<StatusTimeSinceLastTick>()
                .With<StatModifierSetupReference>()
                .With<TargetId>()
                .With<StatusProducedModifiers>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _statuses)
            {
                ref StatusTimeSinceLastTick timeSinceLastTick = ref status.GetComponent<StatusTimeSinceLastTick>();
                ref readonly StatusPeriod statusPeriod = ref status.GetComponent<StatusPeriod>();

                if (timeSinceLastTick.Value >= 0)
                {
                    timeSinceLastTick.Value -= deltaTime;

                    if (status.Has<StatusAffectedMarker>())
                        status.RemoveComponent<StatusAffectedMarker>();
                }
                else
                {
                    ref readonly TargetId target = ref status.GetComponent<TargetId>();
                    ref StatusProducedModifiers statusProducedModifier = ref status.GetComponent<StatusProducedModifiers>();
                    ref StatModifierSetupReference statModifierSetup =
                        ref status.GetComponent<StatModifierSetupReference>();

                    timeSinceLastTick.Value = statusPeriod.Value;

                    Entity modifier = _statsService.CreateStatModifier(target: target.Value,
                        modifierSetup: statModifierSetup.Value,
                        effectTypeOverride: StatEffectType.STATUS, producer: status);

                    statusProducedModifier.Value.Add(modifier);

                    status.AddComponent<StatusAffectedMarker>();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}