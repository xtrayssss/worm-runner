//using _Project.Scripts.Gameplay.Features.CriticalHitFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using _Project.Scripts.Gameplay.Features.StatusFeature.Components;
using Scellecs.Morpeh;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Services
{
    public sealed class ProjectileEnhancements
    {
        private Entity _projectile;

        private ProjectileEnhancements()
        {
        }

        public static ProjectileEnhancements Create() =>
            new ProjectileEnhancements();

        public ProjectileEnhancements ForProjectile(Entity projectile)
        {
            _projectile = projectile;
            return this;
        }

        // public ProjectileEnhancements WithCriticalFrom(Entity source)
        // {
        //     if (!source.Has<CriticalHit>() || _projectile.Has<CriticalHit>())
        //         return this;
        //
        //     ref CriticalHit sourceCriticalHit = ref source.GetComponent<CriticalHit>();
        //
        //     ref Stats stats = ref _projectile.GetComponent<Stats>();
        //     Stat critChance = stats.AddCritChance(sourceCriticalHit.Chance);
        //     Stat critMultiplier = stats.AddCritMultiplier(sourceCriticalHit.Multiplier);
        //
        //     _projectile.AddComponent<CriticalHit>() = new CriticalHit
        //     {
        //         Chance = critChance.CurrentValue,
        //         Multiplier = critMultiplier.CurrentValue,
        //         IsGuaranteed = sourceCriticalHit.IsGuaranteed
        //     };
        //
        //     return this;
        // }

        // public ProjectileEnhancements WithGuaranteedCriticalFrom(Entity source)
        // {
        //     if (!_projectile.Has<GuaranteedCriticalHitSource>())
        //         _projectile.AddComponent<GuaranteedCriticalHitSource>().Value = source;
        //
        //     return this;
        // }

        public ProjectileEnhancements WithStatModifiersFrom(Entity source)
        {
            if (source.Has<StatModifierSetups>())
            {
                ref readonly StatModifierSetups sourceStatModifierSetups =
                    ref source.GetComponent<StatModifierSetups>();
                
                ref StatModifierSetups projectileStatModifierSetups =
                    ref _projectile.AddComponent<StatModifierSetups>();

                projectileStatModifierSetups.Value = sourceStatModifierSetups.Value;
            }
            
            return this;
        }

        public ProjectileEnhancements WithStatusesFrom(Entity source)
        {
            if (source.Has<StatusSetups>())
            {
                ref readonly StatusSetups sourceStatusSetups = ref source.GetComponent<StatusSetups>();
                ref StatusSetups projectileStatusSetups = ref _projectile.AddComponent<StatusSetups>();

                projectileStatusSetups.Value = sourceStatusSetups.Value;
            }

            return this;
        }

        public ProjectileEnhancements WithFreezeChance(Entity source)
        {
            if (!source.Has<FreezeStatusChance>())
                return this;

            ref readonly FreezeStatusChance sourceFreezeChance = ref source.GetComponent<FreezeStatusChance>();
            ref Stats stats = ref _projectile.GetComponent<Stats>();
            Stat freezeChance = stats.AddFreezeChance(sourceFreezeChance.Chance);

            _projectile.AddComponent<FreezeStatusChance>() = new FreezeStatusChance
            {
                Chance = freezeChance.CurrentValue,
                FreezeStatusSetup = sourceFreezeChance.FreezeStatusSetup
            };

            return this;
        }
    }
}