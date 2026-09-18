using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
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
    public sealed class AddStatModifierSystem : ISystem
    {
        public World World { get; set; }
        private Filter _modificationRequests;
        private readonly StatsService _statService;

        public AddStatModifierSystem(StatsService statService) =>
            _statService = statService;

        public void OnAwake()
        {
            _modificationRequests = World.Filter
                .With<TargetId>()
                .With<Producer>()
                .With<AddStatModifierRequest>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity modifier in _modificationRequests)
            {
                if (!TryProcessModifier(modifier))
                    World.RemoveEntity(modifier);
            }
        }

        private bool TryProcessModifier(Entity modifierEntity)
        {
            ref TargetId target = ref modifierEntity.GetComponent<TargetId>();
            ref Producer producer = ref modifierEntity.GetComponent<Producer>();
            ref AddStatModifierRequest request = ref modifierEntity.GetComponent<AddStatModifierRequest>();
            ref Stats stats = ref target.Value.GetComponent<Stats>();

            if (!stats.Value.TryGetValue(request.Modifier.TargetStatId, out Stat stat))
                return false;

            if (!target.Value.Has<ImmortalMarker>())
                ClampModifierToStatBounds(ref request.Modifier, ref stat);

            if (!target.Value.Has<ImmortalMarker>() && !IsModifierEffective(ref stat, in request.Modifier))
                return false;

            bool suppressDamageEvent = modifierEntity.Has<SuppressDamageEventMarker>();

            ApplyModifier(
                ref stat,
                in request,
                target.Value,
                producer.Value,
                modifierEntity,
                request.Modifier.TargetStatId,
                request.Modifier.AffectsBaseValue,
                StatChangeSource.MODIFIER_ADDED,
                request.CustomMinValue,
                suppressDamageEvent);

            return true;
        }

        private void ApplyModifier(ref Stat stat, in AddStatModifierRequest request,
            Entity target, Entity producer, Entity modifier, StatId statId,
            bool affectsBaseValue, StatChangeSource source, float? customMinValue,
            bool suppressDamageEvent)
        {
            if (request.Modifier.AffectsBaseValue)
            {
                stat.ActiveBaseModifiers.Add(request.Modifier);
                RecalculateBaseValue(ref stat, target, producer, modifier, statId,
                    true, source, customMinValue, suppressDamageEvent);
            }
            else
            {
                stat.ActiveCurrentModifiers.Add(request.Modifier);
            }

            RecalculateCurrentValue(ref stat, target, producer, modifier, statId,
                false, source, customMinValue, suppressDamageEvent);
        }

        private void RecalculateCurrentValue(ref Stat stat, Entity target, Entity producer,
            Entity modifier, StatId statId, bool affectsBaseValue, StatChangeSource source,
            float? customMinValue, bool suppressDamageEvent)
        {
            float oldValue = stat.CurrentValue;
            float newValue = _statService.ApplyModifiers(stat.CachedModifiedBaseValue, stat.ActiveCurrentModifiers);
            float minValue = customMinValue ?? stat.MinValue;

            if (!target.Has<ImmortalMarker>())
            {
                stat.CurrentValue = Mathf.Clamp(
                    newValue,
                    min: minValue,
                    max: stat.UseBaseValueAsMaximum ? stat.CachedModifiedBaseValue : stat.MaxValue);
            }
            else
            {
                stat.CurrentValue = newValue;
            }

            TryCreateStatChangeEvent(oldValue, stat.CurrentValue, target, producer, modifier,
                statId, false, source, customMinValue, suppressDamageEvent);
        }

        private void RecalculateBaseValue(ref Stat stat, Entity target, Entity producer,
            Entity modifier, StatId statId, bool affectsBaseValue, StatChangeSource source,
            float? customMinValue, bool suppressDamageEvent)
        {
            float oldValue = stat.CachedModifiedBaseValue;
            float newValue = _statService.ApplyModifiers(stat.BaseValue, stat.ActiveBaseModifiers);

            if (!target.Has<ImmortalMarker>())
            {
                float minValue = customMinValue ?? stat.MinValue;
                stat.CachedModifiedBaseValue = Mathf.Clamp(
                    newValue,
                    min: minValue,
                    max: stat.MaxValue);
            }
            else
            {
                stat.CachedModifiedBaseValue = newValue;
            }

            TryCreateStatChangeEvent(oldValue, stat.CachedModifiedBaseValue, target, producer,
                modifier, statId, true, source, customMinValue, suppressDamageEvent);
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

        private bool IsModifierEffective(ref Stat stat, in StatModifier modifier)
        {
            if (modifier.Operation == StatOperation.ADD && Mathf.Approximately(modifier.Value, 0f))
                return false;

            if (modifier.Operation == StatOperation.MULTIPLY && Mathf.Approximately(modifier.Value, 0f))
                return false;

            if (modifier.AffectsBaseValue)
            {
                switch (modifier.Operation)
                {
                    case StatOperation.ADD:
                    {
                        if ((modifier.Value < 0 && Mathf.Approximately(stat.CachedModifiedBaseValue, stat.MinValue)) ||
                            (modifier.Value > 0 && Mathf.Approximately(stat.CachedModifiedBaseValue, stat.MaxValue)))
                            return false;

                        break;
                    }
                    case StatOperation.MULTIPLY:
                    {
                        if (Mathf.Approximately(stat.BaseValue, 0f))
                            return false;

                        break;
                    }
                    case StatOperation.OVERRIDE:
                    {
                        if (Mathf.Approximately(modifier.Value, stat.CachedModifiedBaseValue) ||
                            (modifier.Value < stat.MinValue &&
                             Mathf.Approximately(stat.CachedModifiedBaseValue, stat.MinValue)) ||
                            (modifier.Value > stat.MaxValue &&
                             Mathf.Approximately(stat.CachedModifiedBaseValue, stat.MaxValue)))
                            return false;
                        break;
                    }
                }
            }
            else
            {
                float maxLimit = stat.UseBaseValueAsMaximum ? stat.CachedModifiedBaseValue : stat.MaxValue;

                switch (modifier.Operation)
                {
                    case StatOperation.ADD:
                    {
                        if ((modifier.Value < 0 && Mathf.Approximately(stat.CurrentValue, stat.MinValue)) ||
                            (modifier.Value > 0 && Mathf.Approximately(stat.CurrentValue, maxLimit)))
                            return false;

                        break;
                    }
                    case StatOperation.MULTIPLY:
                    {
                        if (Mathf.Approximately(stat.CachedModifiedBaseValue, 0f))
                            return false;

                        break;
                    }
                    case StatOperation.OVERRIDE:
                    {
                        if (Mathf.Approximately(modifier.Value, stat.CurrentValue) ||
                            (modifier.Value < stat.MinValue && Mathf.Approximately(stat.CurrentValue, stat.MinValue)) ||
                            (modifier.Value > maxLimit && Mathf.Approximately(stat.CurrentValue, maxLimit)))
                            return false;

                        break;
                    }
                }
            }

            return true;
        }

        private void ClampModifierToStatBounds(ref StatModifier modifier, ref Stat stat)
        {
            float minValue = stat.MinValue;
            float maxValue = stat.MaxValue;

            if (modifier.AffectsBaseValue)
            {
                switch (modifier.Operation)
                {
                    case StatOperation.ADD:
                    {
                        float resultValue = stat.CachedModifiedBaseValue + modifier.Value;
                        if (resultValue > maxValue)
                            modifier.Value = maxValue - stat.CachedModifiedBaseValue;
                        else if (resultValue < minValue)
                            modifier.Value = minValue - stat.CachedModifiedBaseValue;

                        break;
                    }
                    case StatOperation.OVERRIDE:
                    {
                        modifier.Value = Mathf.Clamp(modifier.Value, minValue, maxValue);
                        break;
                    }
                    case StatOperation.MULTIPLY:
                    {
                        if (modifier.Value >= 0)
                        {
                            float resultValue = stat.CachedModifiedBaseValue +
                                                (stat.CachedModifiedBaseValue * modifier.Value);
                            if (resultValue > maxValue)
                                modifier.Value = (maxValue - stat.CachedModifiedBaseValue) /
                                                 stat.CachedModifiedBaseValue;
                        }
                        else
                        {
                            float resultValue = stat.CachedModifiedBaseValue +
                                                (stat.CachedModifiedBaseValue * modifier.Value);
                            if (resultValue < minValue)
                                modifier.Value = (minValue - stat.CachedModifiedBaseValue) /
                                                 stat.CachedModifiedBaseValue;
                        }

                        break;
                    }
                }
            }
            else
            {
                float upperLimit = stat.UseBaseValueAsMaximum ? stat.CachedModifiedBaseValue : maxValue;

                switch (modifier.Operation)
                {
                    case StatOperation.ADD:
                    {
                        float resultValue = stat.CurrentValue + modifier.Value;
                        if (resultValue > upperLimit)
                            modifier.Value = upperLimit - stat.CurrentValue;
                        else if (resultValue < minValue)
                            modifier.Value = minValue - stat.CurrentValue;
                        break;
                    }
                    case StatOperation.OVERRIDE:
                    {
                        modifier.Value = Mathf.Clamp(modifier.Value, minValue, upperLimit);
                        break;
                    }
                    case StatOperation.MULTIPLY:
                    {
                        if (modifier.Value >= 0)
                        {
                            float resultValue = stat.CurrentValue + (stat.CurrentValue * modifier.Value);
                            if (resultValue > upperLimit)
                                modifier.Value = (upperLimit - stat.CurrentValue) / stat.CurrentValue;
                        }
                        else
                        {
                            float resultValue = stat.CurrentValue + (stat.CurrentValue * modifier.Value);
                            if (resultValue < minValue)
                                modifier.Value = (minValue - stat.CurrentValue) / stat.CurrentValue;
                        }

                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}