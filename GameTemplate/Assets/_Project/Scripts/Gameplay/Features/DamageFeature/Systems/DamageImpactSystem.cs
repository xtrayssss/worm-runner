using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DamageImpactSystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvent;

        public void OnAwake() =>
            _damagedEvent = World.GetEvent<DamagedEvent>();

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvent.publishedChanges)
            {
                 Entity producer =  damagedEvent.Modifier.Producer;

                if (producer.IsNullOrDisposed())
                    continue;

                UpdateDamageImpact(producer, damagedEvent, isDirectDamageDealer: true);

                if (!producer.Has<Owner>() || producer.GetComponent<Owner>().Value.IsNullOrDisposed())
                    continue;

                ref readonly Owner owner = ref producer.GetComponent<Owner>();

                UpdateDamageImpact(owner.Value, damagedEvent);
            }
        }

        private void UpdateDamageImpact(Entity entity, in DamagedEvent damagedEvent, bool isDirectDamageDealer = false)
        {
            if (entity.Has<DamageImpactEvent>())
            {
                ref DamageImpactEvent damageImpactEvent = ref entity.GetComponent<DamageImpactEvent>();
                damageImpactEvent.ImpactCount++;
                damageImpactEvent.TotalDamage += damagedEvent.DamageAmount;
                damageImpactEvent.Targets.Add(damagedEvent.Damageable);
                damageImpactEvent.StatEffectType = damagedEvent.Modifier.StatEffectType;
                damageImpactEvent.IsCritical = damagedEvent.Modifier.IsCritical;
                // damageImpactEvent.IsHunterDamaged = damagedEvent.IsHunterDamaged;
            }
            else
            {
                entity.AddComponent<DamageImpactEvent>() = new DamageImpactEvent
                {
                    ImpactCount = 1,
                    FirstTarget = damagedEvent.Damageable,
                    Targets = new HashSet<Entity> { damagedEvent.Damageable },
                    Point = damagedEvent.Modifier.EffectPoint,
                    TotalDamage = damagedEvent.DamageAmount,
                    IsDirectDamageDealer = isDirectDamageDealer,
                    StatEffectType = damagedEvent.Modifier.StatEffectType,
                    IsCritical = damagedEvent.Modifier.IsCritical,
                    // IsHunterDamaged = damagedEvent.IsHunterDamaged
                };
            }
        }

        public void Dispose()
        {
        }
    }
}