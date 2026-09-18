using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RemoveStatModifierSystem : ISystem
    {
        public World World { get; set; }
        private Filter _removedModifiers;
        private readonly StatsService _statService;

        public RemoveStatModifierSystem(StatsService statService) =>
            _statService = statService;

        public void OnAwake()
        {
            _removedModifiers = World.Filter
                .With<TargetId>()
                .With<Producer>()
                .With<RemoveStatModifierRequest>()
                .Without<RemovedModifierMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity removedModifier in _removedModifiers)
            {
                ProcessModifierRemoval(removedModifier);
            }
        }

        private void ProcessModifierRemoval(Entity modifierEntity)
        {
            ref TargetId target = ref modifierEntity.GetComponent<TargetId>();
            ref Producer producer = ref modifierEntity.GetComponent<Producer>();
            ref readonly RemoveStatModifierRequest request =
                ref modifierEntity.GetComponent<RemoveStatModifierRequest>();
            ref Stats stats = ref target.Value.GetComponent<Stats>();

            if (!stats.Value.TryGetValue(request.StatId, out Stat stat))
                return;

            modifierEntity.AddComponent<RemovedModifierMarker>();

            bool affectsBaseValue = modifierEntity.Has<AffectsBaseValueMarker>();
            var targetModifiers = affectsBaseValue ? stat.ActiveBaseModifiers : stat.ActiveCurrentModifiers;

            if (!RemoveModifierById(targetModifiers, request.Id))
                return;

            bool suppressDamageEvent = modifierEntity.Has<SuppressDamageEventMarker>();

            RecalculateStats(
                ref stat,
                target.Value,
                producer.Value,
                modifierEntity,
                request.StatId,
                affectsBaseValue, 
                StatChangeSource.MODIFIER_REMOVED,
                request.CustomMinValue,
                suppressDamageEvent);
        }

        private void RecalculateStats(ref Stat stat, Entity target, Entity producer,
            Entity modifier, StatId statId, bool affectsBaseValue, StatChangeSource source,
            float? customMinValue, bool suppressDamageEvent)
        {
            if (affectsBaseValue)
                RecalculateBaseValue(
                    ref stat,
                    target,
                    producer,
                    modifier,
                    statId,
                    true,
                    source,
                    customMinValue,
                    suppressDamageEvent);

            RecalculateCurrentValue(
                ref stat,
                target,
                producer,
                modifier,
                statId,
                false,
                source,
                customMinValue,
                suppressDamageEvent);
        }

        private void RecalculateBaseValue(ref Stat stat, Entity target, Entity producer,
            Entity modifier, StatId statId, bool affectsBaseValue, StatChangeSource source,
            float? customMinValue, bool suppressDamageEvent)
        {
            float oldValue = stat.CachedModifiedBaseValue;
            float newValue = _statService.ApplyModifiers(stat.BaseValue, stat.ActiveBaseModifiers);
            float minValue = customMinValue ?? stat.MinValue;

            stat.CachedModifiedBaseValue = Mathf.Clamp(newValue, minValue, stat.MaxValue);

            TryCreateStatChangeEvent(oldValue, stat.CachedModifiedBaseValue, target, producer,
                modifier, statId, true, source, customMinValue, suppressDamageEvent);
        }

        private void RecalculateCurrentValue(ref Stat stat, Entity target, Entity producer,
            Entity modifier, StatId statId, bool affectsBaseValue, StatChangeSource source,
            float? customMinValue, bool suppressDamageEvent)
        {
            float oldValue = stat.CurrentValue;
            float newValue = _statService.ApplyModifiers(stat.CachedModifiedBaseValue, stat.ActiveCurrentModifiers);
            float minValue = customMinValue ?? stat.MinValue;
            float maxValue = stat.UseBaseValueAsMaximum ? stat.CachedModifiedBaseValue : stat.MaxValue;

            stat.CurrentValue = Mathf.Clamp(newValue, minValue, maxValue);

            TryCreateStatChangeEvent(oldValue, stat.CurrentValue, target, producer, modifier,
                statId, false, source, customMinValue, suppressDamageEvent);
        }

        private void TryCreateStatChangeEvent(float oldValue, float newValue, Entity target,
            Entity producer, Entity modifier, StatId statId, bool affectsBaseValue,
            StatChangeSource source, float? customMinValue, bool suppressDamageEvent)
        {
            if (!Mathf.Approximately(oldValue, newValue))
            {
                _statService.CreateStatChangeEvent(
                    World,
                    target,
                    producer,
                    statId,
                    oldValue,
                    newValue,
                    affectsBaseValue,
                    modifier,
                    customMinValue,
                    suppressDamageEvent,
                    source);
            }
        }

        private static bool RemoveModifierById(List<StatModifier> modifiers, int id)
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].Id == id)
                {
                    modifiers.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
        }
    }
}