using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Services
{
    public sealed class StatsService : IService
    {
        private readonly ConfigsService _configs;
        private int _modifierIdCounter;

        public static readonly HashSet<StatId> INVERSE_BENEFICIAL_STATS = new HashSet<StatId>
        {
            StatId.COOLDOWN,
            StatId.HITS_FOR_CRIT
        };

        public StatsService(ConfigsService configs) =>
            _configs = configs;

        public float ApplyMultiplier(float baseValue, float multiplier)
        {
            return baseValue * (1 + multiplier);
        }

        public float ApplyModifiers(float baseValue, List<StatModifier> modifiers)
        {
            float currentValue = baseValue;

            modifiers.Sort(static (a, b) => a.Priority.CompareTo(b.Priority));

            foreach (StatModifier modifier in modifiers)
            {
                switch (modifier.Operation)
                {
                    case StatOperation.ADD:
                        currentValue += modifier.Value;
                        break;
                    case StatOperation.MULTIPLY:
                        currentValue += baseValue * modifier.Value;
                        break;
                    case StatOperation.OVERRIDE:
                        return modifier.Value;
                }
            }

            return currentValue;
        }

        public Entity CreateEffectModifier(Entity target,
            StatId targetStat,
            StatOperation operation,
            float modifierValue,
            Entity producer = default,
            Vector3? position = null,
            bool suppressDamageEvent = false)
        {
            SimpleStatModifierSetup modifierSetup = new SimpleStatModifierSetup
            {
                TargetStatId = targetStat,
                Operation = operation,
                ModifierValue = modifierValue,
                Priority = (int)operation,
                EffectType = StatEffectType.INSTANT,
                AffectsBaseValue = false
            };

            return CreateStatModifierInternal(
                target,
                producer,
                modifierSetup,
                position ?? GetTargetPosition(target),
                statSource: default,
                suppressDamageEvent,
                customMinValue: null);
        }

        public Entity CreateStatModifier(Entity target,
            StatModifierSetup modifierSetup,
            Entity statSource = default,
            Vector3? point = null,
            StatEffectType? effectTypeOverride = null,
            Entity producer = default,
            bool suppressDamageEvent = false,
            float? customMinValue = null)
        {
            return CreateStatModifierInternal(
                target,
                producer,
                modifierSetup,
                point ?? GetTargetPosition(target),
                statSource,
                suppressDamageEvent,
                customMinValue,
                effectTypeOverride);
        }

        private Entity CreateStatModifierInternal(Entity target,
            Entity producer,
            StatModifierSetup modifierSetup,
            Vector3 point,
            Entity statSource,
            bool suppressDamageEvent,
            float? customMinValue,
            StatEffectType? effectTypeOverride = null)
        {
            Entity modifier = World.Default.CreateEntity();
            modifier.AddComponent<StatModifierTag>();

            ref ModifierValue modifierValue = ref modifier.AddComponent<ModifierValue>();

            if (!statSource.IsNullOrDisposed() && modifierSetup.SourceStatId != StatId.UNKNOWN)
                modifierValue.Value = statSource.GetComponent<Stats>().Value[modifierSetup.SourceStatId].CurrentValue;
            else
                modifierValue.Value = modifierSetup.ModifierValue;

            if (modifierValue.Value < 0)
                modifier.AddComponent<DecreaseModifierMarker>();
            else if (modifierValue.Value > 0)
                modifier.AddComponent<IncreaseModifierMarker>();

            if (modifierSetup.AffectsBaseValue)
                modifier.AddComponent<AffectsBaseValueMarker>();

            if (suppressDamageEvent)
                modifier.AddComponent<SuppressDamageEventMarker>();

            int nextModifierId = GetNextModifierId();

            modifier.AddComponent<AddStatModifierRequest>() = new AddStatModifierRequest
            {
                Modifier = new StatModifier
                {
                    TargetStatId = modifierSetup.TargetStatId,
                    Value = modifierValue.Value,
                    Operation = modifierSetup.Operation,
                    Priority = modifierSetup.Priority,
                    AffectsBaseValue = modifierSetup.AffectsBaseValue,
                    Id = nextModifierId,
                    ModifierEntity = modifier
                },
                CustomMinValue = customMinValue
            };

            modifier.AddComponent<Producer>().Value = producer;
            modifier.AddComponent<TargetId>().Value = target;
            modifier.AddComponent<ImpactPoint>().Value = point;

            StatEffectType statEffectType = effectTypeOverride ?? modifierSetup.EffectType;
            modifier.AddComponent<EffectTypeReference>().Value = statEffectType;

            modifier.AddComponent<ModifierId>() = new ModifierId
            {
                Id = nextModifierId,
                StatId = modifierSetup.TargetStatId
            };

#if DEBUG
            string modifierTypeName = modifierSetup.AffectsBaseValue ? "Upgrade" : "Effect";
            modifier.AddComponent<EntityName>().Value =
                $"[{modifierTypeName}] {modifierSetup.TargetStatId} {modifierSetup.Operation} {modifierSetup.ModifierValue}";
#endif

            modifierSetup.Compose(modifier);

            return modifier;
        }

        public void RemoveModifier(Entity modifier, float? customMinValue = null)
        {
            if (!modifier.IsNullOrDisposed() && !modifier.Has<DeathModifierMarker>())
            {
                modifier.AddComponent<DeathModifierMarker>();

                if (customMinValue != null)
                {
                    modifier.AddComponent<CustomMinValueOnRemove>() = new CustomMinValueOnRemove
                    {
                        Value = customMinValue
                    };
                }
            }
        }

        public void RemoveUpgradeModifiers(Entity target, StatId statId, Entity producer = default)
        {
            ClearStatModifiers(target, statId, clearBase: true, clearCurrent: false, producer);
        }

        public void RemoveEffectModifiers(Entity target, StatId statId, Entity producer = default)
        {
            ClearStatModifiers(target, statId, clearBase: false, clearCurrent: true, producer);
        }

        public void RemoveAllStatModifiers(Entity target, StatId statId, Entity producer = default)
        {
            ClearStatModifiers(target, statId, clearBase: true, clearCurrent: true, producer);
        }


        public void RemoveAllModifiersFromEntity(Entity target, Entity producer = default)
        {
            ClearStatModifiers(target, StatId.UNKNOWN, clearBase: true, clearCurrent: true, producer);
        }

        private void ClearStatModifiers(
            Entity target,
            StatId statId,
            bool clearBase,
            bool clearCurrent,
            Entity producer = default)
        {
            if (producer.IsNullOrDisposed())
                producer = target;

            World.Default
                .GetRequest<ClearStatModifiersRequest>()
                .Publish(new ClearStatModifiersRequest
                {
                    Target = target,
                    Producer = producer,
                    StatId = statId,
                    ClearBaseModifiers = clearBase,
                    ClearCurrentModifiers = clearCurrent
                });
        }

        public Entity CreateStatChangeEvent(World world,
            Entity target,
            Entity producer,
            StatId statId,
            float oldValue,
            float newValue,
            bool affectsBaseValue,
            Entity modifier,
            float? customMinValue,
            bool suppressDamageEvent,
            StatChangeSource source = StatChangeSource.OTHER)
        {
            Entity eventEntity = world.CreateEntity();

            eventEntity.AddComponent<StatChangedEvent>() = new StatChangedEvent
            {
                Producer = producer,
                Target = target,
                StatId = statId,
                OldValue = oldValue,
                NewValue = newValue,
                Modifier = modifier,
                Source = source,
                SuppressDamageEvent = suppressDamageEvent
            };

            return eventEntity;
        }

        private Vector3 GetTargetPosition(Entity target) =>
            target.Has<EntityViewLink>()
                ? target.GetComponent<EntityViewLink>().View.transform.position
                : Vector3.zero;

        private int GetNextModifierId() =>
            _modifierIdCounter++;
    }
}