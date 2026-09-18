using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.StatsFeature.Services
{
    public class StatsBuilder
    {
        private class StatParams
        {
            public float BaseValue;
            public float? CurrentValue;
            public float? MaxValue;
            public float? MinValue;
            public bool UseBaseValueAsMaximum;
        }

        private readonly Dictionary<StatId, StatParams> _stats = new Dictionary<StatId, StatParams>();

        public StatsBuilder SetStat(
            StatId type,
            float baseValue,
            float? currentValue = null,
            float? maxValue = null,
            float? minValue = null,
            bool useBaseValueAsMaximum = false)
        {
            _stats[type] = new StatParams
            {
                BaseValue = baseValue,
                CurrentValue = currentValue,
                MaxValue = maxValue,
                MinValue = minValue,
                UseBaseValueAsMaximum = useBaseValueAsMaximum
            };

            return this;
        }

        public StatsBuilder SetStats(
            params (StatId Type, float BaseValue, float? CurrentValue, float? MaxValue, float? MinValue, bool
                useBaseValueAsMaximum)[] stats)
        {
            foreach ((StatId Type, float BaseValue, float? CurrentValue, float? MaxValue, float? MinValue, bool
                     useBaseValueAsMaximum) stat in stats)
            {
                SetStat(stat.Type, stat.BaseValue, stat.CurrentValue, stat.MaxValue, stat.MinValue,
                    stat.useBaseValueAsMaximum);
            }

            return this;
        }

        public StatsBuilder SetDamage(float baseValue, float? minValue = null) =>
            SetStat(StatId.DAMAGE, baseValue, minValue: minValue ?? -int.MaxValue);

        public StatsBuilder SetHealth(float baseValue) =>
            SetStat(StatId.HEALTH, baseValue, useBaseValueAsMaximum: true);

        public StatsBuilder SetDefense(float baseValue, float? maxValue = null) =>
            SetStat(StatId.DEFENSE, baseValue, maxValue: maxValue ?? int.MaxValue);

        public StatsBuilder SetCritChance(float baseValue) =>
            SetStat(StatId.CRIT_CHANCE, baseValue, maxValue: GetCriticalHitChanceMaxValue());

        public StatsBuilder SetFreezeChance(float baseValue) =>
            SetStat(StatId.FREEZE_CHANCE, baseValue, maxValue: GetFreezeChanceMaxValue());

        public StatsBuilder SetShotsPerAttack(float baseValue) =>
            SetStat(StatId.SHOTS_PER_ATTACK, baseValue, minValue: 1);

        public StatsBuilder SetCritMultiplier(float baseValue) =>
            SetStat(StatId.CRIT_MULTIPLIER, baseValue);

        public StatsBuilder SetHealthRegen(float baseValue) =>
            SetStat(StatId.HEALTH_REGEN, baseValue);

        public StatsBuilder SetDefenseRegen(float baseValue) =>
            SetStat(StatId.DEFENSE_REGEN, baseValue);

        public StatsBuilder SetBaseHealthBonus(float baseValue) =>
            SetStat(StatId.BASE_HEALTH_BONUS, baseValue);

        public StatsBuilder SetBaseDefenseBonus(float baseValue) =>
            SetStat(StatId.BASE_DEFENSE_BONUS, baseValue);

        public StatsBuilder SetPierce(float baseValue) =>
            SetStat(StatId.PIERCE, baseValue);

        public StatsBuilder SetRicochet(float baseValue) =>
            SetStat(StatId.RICOCHET, baseValue);

        public Stats Build()
        {
            return new Stats
            {
                Value = _stats.ToDictionary(
                    static stat => stat.Key,
                    static stat =>
                    {
                        (StatId statId, StatParams statParams) = stat;
                        return CreateStatByType(statId, statParams);
                    })
            };
        }

        private static Stat CreateStatByType(StatId statId, StatParams statParams)
        {
            return statId switch
            {
                StatId.CRIT_CHANCE => CreateStat(statParams.BaseValue, maxValue: GetCriticalHitChanceMaxValue()),
                StatId.FREEZE_CHANCE => CreateStat(statParams.BaseValue, maxValue: GetFreezeChanceMaxValue()),
                StatId.HEALTH => CreateStat(statParams.BaseValue, useBaseValueAsMaximum: statParams.UseBaseValueAsMaximum),
                StatId.SHOTS_PER_ATTACK => CreateStat(statParams.BaseValue, minValue: 1),
                StatId.DAMAGE => CreateStat(statParams.BaseValue, minValue: statParams.MinValue ?? -int.MaxValue),
                _ => CreateStat(
                    baseValue: statParams.BaseValue,
                    currentValue: statParams.CurrentValue,
                    maxValue: statParams.MaxValue,
                    minValue: statParams.MinValue,
                    useBaseValueAsMaximum: statParams.UseBaseValueAsMaximum)
            };
        }

        public static Stat CreateStat(
            float baseValue,
            float? currentValue = null,
            float? maxValue = null,
            float? minValue = null,
            bool useBaseValueAsMaximum = false)
        {
            return new Stat
            {
                BaseValue = baseValue,
                CurrentValue = currentValue ?? baseValue,
                MaxValue = maxValue ?? float.MaxValue,
                MinValue = minValue ?? 0,
                CachedModifiedBaseValue = baseValue,
                UseBaseValueAsMaximum = useBaseValueAsMaximum
            };
        }

        public static float GetCriticalHitChanceMaxValue() => 0.5f;
        public static float GetFreezeChanceMaxValue() => 0.8f;
    }

    public static class StatsExtensions
    {
        public static bool EnsureStatExists(Entity entity, StatId statId, float defaultBaseValue = 0f)
        {
            if (!entity.Has<Stats>())
                entity.AddComponent<Stats>() = new Stats { Value = new Dictionary<StatId, Stat>() };

            ref Stats stats = ref entity.GetComponent<Stats>();

            if (stats.Value.ContainsKey(statId))
                return true;

            switch (statId)
            {
                case StatId.HEALTH:
                    stats.AddHealth(defaultBaseValue);
                    break;
                case StatId.DAMAGE:
                    stats.AddDamage(defaultBaseValue);
                    break;
                case StatId.DEFENSE:
                    stats.AddStat(StatId.DEFENSE, defaultBaseValue);
                    break;
                case StatId.CRIT_CHANCE:
                    stats.AddCritChance(defaultBaseValue);
                    break;
                case StatId.FREEZE_CHANCE:
                    stats.AddFreezeChance(defaultBaseValue);
                    break;
                case StatId.SHOTS_PER_ATTACK:
                    stats.AddShotsPerAttack(defaultBaseValue);
                    break;
                case StatId.CRIT_MULTIPLIER:
                    stats.AddCritMultiplier(defaultBaseValue);
                    break;
                case StatId.HEALTH_REGEN:
                    stats.AddHealthRegen(defaultBaseValue);
                    break;
                case StatId.DEFENSE_REGEN:
                    stats.AddDefenseRegen(defaultBaseValue);
                    break;
                case StatId.BASE_HEALTH_BONUS:
                    stats.AddBaseHealthBonus(defaultBaseValue);
                    break;
                case StatId.BASE_DEFENSE_BONUS:
                    stats.AddBaseDefenseBonus(defaultBaseValue);
                    break;
                case StatId.PIERCE:
                    stats.AddPierce(defaultBaseValue);
                    break;
                case StatId.RICOCHET:
                    stats.AddRicochet(defaultBaseValue);
                    break;
                case StatId.UNKNOWN:
                case StatId.SPEED:
                case StatId.COOLDOWN:
                case StatId.HITS_FOR_CRIT:
                default:
                    throw new ArgumentException($"Stat {statId} does not exist");
            }

            return true;
        }

        public static Stat AddStat(this Stats stats, StatId statId, float baseValue,
            float? currentValue = null, float? maxValue = null, float? minValue = null,
            bool useBaseValueAsMaximum = false)
        {
            Stat stat = StatsBuilder.CreateStat(baseValue, currentValue, maxValue, minValue, useBaseValueAsMaximum);
            stats.Value.Add(statId, stat);
            return stat;
        }

        public static void AddStats(this Stats stats,
            params (StatId Type, float BaseValue, float? CurrentValue, float? MaxValue, float? MinValue, bool
                useBaseValueAsMaximum)[] statParams)
        {
            foreach ((StatId Type, float BaseValue, float? CurrentValue, float? MaxValue, float? MinValue, bool
                     useBaseValueAsMaximum) param in statParams)
            {
                stats.AddStat(param.Type, param.BaseValue, param.CurrentValue, param.MaxValue, param.MinValue,
                    param.useBaseValueAsMaximum);
            }
        }

        public static Stat AddDamage(this Stats stats, float baseValue, float? minValue = null) =>
            stats.AddStat(StatId.DAMAGE, baseValue, minValue: minValue ?? -int.MaxValue);

        public static Stat AddHealth(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.HEALTH, baseValue, useBaseValueAsMaximum: true);

        public static Stat AddCritChance(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.CRIT_CHANCE, baseValue, maxValue: StatsBuilder.GetCriticalHitChanceMaxValue());

        public static Stat AddFreezeChance(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.FREEZE_CHANCE, baseValue, maxValue: StatsBuilder.GetFreezeChanceMaxValue());

        public static Stat AddShotsPerAttack(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.SHOTS_PER_ATTACK, baseValue, minValue: 1);

        public static Stat AddCritMultiplier(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.CRIT_MULTIPLIER, baseValue);

        public static Stat AddHealthRegen(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.HEALTH_REGEN, baseValue);

        public static Stat AddDefenseRegen(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.DEFENSE_REGEN, baseValue);

        public static Stat AddBaseHealthBonus(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.BASE_HEALTH_BONUS, baseValue);

        public static Stat AddBaseDefenseBonus(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.BASE_DEFENSE_BONUS, baseValue);

        public static Stat AddPierce(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.PIERCE, baseValue);

        public static Stat AddRicochet(this Stats stats, float baseValue) =>
            stats.AddStat(StatId.RICOCHET, baseValue);
    }

    public static class FluentStatsExtensions
    {
        public static ref Stats WithStat(this ref Stats stats, StatId statId, float baseValue,
            float? currentValue = null, float? maxValue = null, float? minValue = null,
            bool useBaseValueAsMaximum = false)
        {
            stats.AddStat(statId, baseValue, currentValue, maxValue, minValue, useBaseValueAsMaximum);
            return ref stats;
        }

        public static ref Stats WithStats(this ref Stats stats,
            params (StatId Type, float BaseValue, float? CurrentValue, float? MaxValue, float? MinValue, bool
                useBaseValueAsMaximum)[] statParams)
        {
            stats.AddStats(statParams);
            return ref stats;
        }

        public static ref Stats WithDamage(this ref Stats stats, float baseValue, float? minValue = null)
        {
            stats.AddDamage(baseValue, minValue);
            return ref stats;
        }

        public static ref Stats WithAttackCooldown(this ref Stats stats, float baseValue)
        {
            stats.AddStat(StatId.COOLDOWN, baseValue);
            return ref stats;
        }
        
        public static ref Stats WithContactDamage(this ref Stats stats, float baseValue)
        {
            stats.AddStat(StatId.CONTACT_DAMAGE, baseValue);
            return ref stats;
        }

        public static ref Stats WithHealth(this ref Stats stats, float baseValue)
        {
            stats.AddHealth(baseValue);
            return ref stats;
        }

        public static ref Stats WithCritChance(this ref Stats stats, float baseValue)
        {
            stats.AddCritChance(baseValue);
            return ref stats;
        }

        public static ref Stats WithFreezeChance(this ref Stats stats, float baseValue)
        {
            stats.AddFreezeChance(baseValue);
            return ref stats;
        }

        public static ref Stats WithShotsPerAttack(this ref Stats stats, float baseValue)
        {
            stats.AddShotsPerAttack(baseValue);
            return ref stats;
        }

        public static ref Stats WithCritMultiplier(this ref Stats stats, float baseValue)
        {
            stats.AddCritMultiplier(baseValue);
            return ref stats;
        }

        public static ref Stats WithHealthRegen(this ref Stats stats, float baseValue)
        {
            stats.AddHealthRegen(baseValue);
            return ref stats;
        }

        public static ref Stats WithDefenseRegen(this ref Stats stats, float baseValue)
        {
            stats.AddDefenseRegen(baseValue);
            return ref stats;
        }

        public static ref Stats WithBaseHealthBonus(this ref Stats stats, float baseValue)
        {
            stats.AddBaseHealthBonus(baseValue);
            return ref stats;
        }

        public static ref Stats WithBaseDefenseBonus(this ref Stats stats, float baseValue)
        {
            stats.AddBaseDefenseBonus(baseValue);
            return ref stats;
        }

        public static ref Stats WithPierce(this ref Stats stats, float baseValue)
        {
            stats.AddPierce(baseValue);
            return ref stats;
        }

        public static ref Stats WithRicochet(this ref Stats stats, float baseValue)
        {
            stats.AddRicochet(baseValue);
            return ref stats;
        }
    }
}