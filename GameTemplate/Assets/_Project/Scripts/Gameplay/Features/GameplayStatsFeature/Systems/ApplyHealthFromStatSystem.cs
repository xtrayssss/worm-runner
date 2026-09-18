using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.GameplayStatsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ApplyHealthFromStatSystem : ISystem
    {
        public World World { get; set; }

        private Filter _statChanges;

        public void OnAwake()
        {
            _statChanges = World.Filter
                .With<StatChangedEvent>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity changeEvent in _statChanges)
            {
                ref readonly StatChangedEvent statChange = ref changeEvent.GetComponent<StatChangedEvent>();

                if (statChange.StatId != StatId.HEALTH || statChange.Target.Has<ZeroHealthMarker>())
                    continue;

                ref Health health = ref statChange.Target.GetComponent<Health>();

                float oldHealth = health.CurrentHealth;
                float delta = statChange.NewValue - statChange.OldValue;

                health.CurrentHealth = statChange.Target.GetComponent<Stats>().Value[StatId.HEALTH].CurrentValue;
                health.MaxHealth = statChange.Target.GetComponent<Stats>().Value[StatId.HEALTH].CachedModifiedBaseValue;

                if (delta < 0 || (health.CurrentHealth <= 0 && oldHealth > 0))
                {
                    DamageSourceType sourceType = DetermineDamageSourceType(statChange.Producer);

                    World.GetEvent<DamagedEvent>()
                        .NextFrame(new DamagedEvent
                        {
                            Damageable = statChange.Target,
                            DamageAmount = delta < 0 ? Mathf.Abs(delta) : oldHealth,
                            HealthChangeSource = statChange.Source,
                            DamageSourceType = sourceType,
                            Modifier = new DamagedEvent.ModifierData
                            {
                                //IsCritical = statChange.Modifier.Has<CriticalHitEffectMarker>(),
                                StatEffectType = statChange.Modifier.GetComponent<EffectTypeReference>().Value,
                                EffectPoint = statChange.Modifier.GetComponent<ImpactPoint>().Value,
                                Producer = statChange.Producer,
                            }
                        });
                }
                else if (delta > 0)
                {
                    // World.CreateEntity().SetComponent(new HealedEvent
                    // {
                    //     Target = statChange.Target,
                    //     Amount = delta
                    // });
                }

                if (health.CurrentHealth <= 0 && !statChange.Target.Has<ImmortalMarker>())
                    statChange.Target.AddComponent<ZeroHealthMarker>();
            }
        }

        public void Dispose()
        {
        }

        private DamageSourceType DetermineDamageSourceType(Entity producer)
        {
            if (producer.IsNullOrDisposed())
                return DamageSourceType.UNKNOWN;

            return producer switch
            {
                _ when producer.Has<ProjectileTag>() => DamageSourceType.PROJECTILE,
                _ when producer.Has<MilitaryCrowdMemberTag>() => DamageSourceType.CROWD_MEMBER,
                _ when producer.Has<MineTag>() => DamageSourceType.MINE,
                _ when producer.Has<CannonTag>() => DamageSourceType.CANNON,
                _ => DamageSourceType.UNKNOWN
            };
        }
    }
}