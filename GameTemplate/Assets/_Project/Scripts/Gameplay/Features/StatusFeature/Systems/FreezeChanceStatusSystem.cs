using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using _Project.Scripts.Gameplay.Features.StatusFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.StatusFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FreezeChanceStatusSystem : ISystem
    {
        public World World { get; set; }

        private Filter _damageEffects;
        private readonly StatusApplier _statusApplier;

        public FreezeChanceStatusSystem(StatusApplier statusApplier) =>
            _statusApplier = statusApplier;

        public void OnAwake()
        {
            _damageEffects = World.Filter
                .With<HealthModifierTag>()
                .With<DecreaseModifierMarker>()
                .With<Producer>()
                .With<TargetId>()
                .Without<ModifierAppliedMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity effect in _damageEffects)
            {
                ref readonly Producer producer = ref effect.GetComponent<Producer>();
                ref readonly TargetId target = ref effect.GetComponent<TargetId>();

                if (producer.Value.IsNullOrDisposed() || !producer.Value.Has<FreezeStatusChance>())
                    continue;

                ref FreezeStatusChance freezeChance = ref producer.Value.GetComponent<FreezeStatusChance>();

                if (Random.Range(0f, 1f) <= freezeChance.Chance)
                {
#if DEBUG
                    Debug.Log(
                        $"Freeze chance succeeded! Applying freeze effect with {freezeChance.Chance * 100}% probability.");
#endif
                    _statusApplier.ApplyStatus(
                        freezeChance.FreezeStatusSetup,
                        producer.Value,
                        target.Value,
                        enhancementSource: producer.Value);
                }
#if DEBUG
                else
                {
                    Debug.Log(
                        $"Freeze chance failed. Roll exceeded {freezeChance.Chance * 100}% probability threshold.");
                }
#endif
            }
        }

        public void Dispose()
        {
        }
    }
}