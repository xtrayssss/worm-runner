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
    public sealed class ApplyFreezeStatusSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statuses;

        private readonly StatsService _statsService;

        public ApplyFreezeStatusSystem(StatsService statsService) =>
            _statsService = statsService;

        public void OnAwake()
        {
            _statuses = World.Filter
                .With<FreezeStatusTag>()
                .With<TargetId>()
                .With<StatusProducedModifiers>()
                .Without<StatusAffectedMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity status in _statuses)
            {
                ref readonly TargetId target = ref status.GetComponent<TargetId>();
                ref StatusProducedModifiers statusProducedModifiers = ref status.GetComponent<StatusProducedModifiers>();

                ref StatModifierSetupReference statModifierSetup =
                    ref status.GetComponent<StatModifierSetupReference>();

                Entity modifier = _statsService.CreateStatModifier(target: target.Value,
                    modifierSetup: statModifierSetup.Value,
                    effectTypeOverride: StatEffectType.STATUS, producer: status);

                statusProducedModifiers.Value.Add(modifier);

                status.AddComponent<StatusAffectedMarker>();
            }
        }

        public void Dispose()
        {
        }
    }
}