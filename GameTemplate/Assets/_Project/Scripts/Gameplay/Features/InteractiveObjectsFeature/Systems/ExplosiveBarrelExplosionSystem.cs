using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ExplosiveBarrelExplosionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _explodedBarrels;
        private readonly StatsService _statsService;

        private readonly Collider[] _targetsBuffer = new Collider[10];
        private Event<DamagedEvent> _damagedEvent;

        public ExplosiveBarrelExplosionSystem(StatsService statsService)
        {
            _statsService = statsService;
        }

        public void OnAwake()
        {
            _damagedEvent = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvent.publishedChanges)
            {
                Entity damageable = damagedEvent.Damageable;

                if (damageable.IsNullOrDisposed()
                    || !damageable.Has<ExplosiveBarrelTag>()
                    || !damageable.Has<ZeroHealthMarker>()
                    || damageable.Has<ExplosiveBarrelExplodedMarker>())
                    continue;

                Entity barrel = damageable;

                ref readonly ExplosiveBarrel explosiveData = ref barrel.GetComponent<ExplosiveBarrel>();
                ref readonly EntityViewLink barrelViewLink = ref barrel.GetComponent<EntityViewLink>();

                Vector3 explosionCenter = barrelViewLink.View.transform.position;

                ExplosiveBarrelView barrelView = (ExplosiveBarrelView)barrelViewLink.View;
                barrelView.PlayExplosionEffect();

                DealExplosionDamage(
                    barrel,
                    explosionCenter,
                    explosiveData.ExplosionRadius,
                    explosiveData.ExplosionDamage);

                barrel.AddComponent<ExplosiveBarrelExplodedMarker>();
                barrel.AddComponent<ExplosiveBarrelDestroyMarker>();
            }
        }

        private void DealExplosionDamage(
            Entity sourceBarrel,
            Vector3 center,
            float radius,
            float damage)
        {
            int damageableMask =
                LayerUtils.CreateMask(LayerUtils.CROWD_MEMBER_LAYER, LayerUtils.INTERACTIVE_OBJECT_LAYER);

            int size = Physics.OverlapSphereNonAlloc(center, radius, _targetsBuffer, damageableMask);

            HashSet<Entity> processedEntities = new HashSet<Entity>();

            for (int i = 0; i < size; i++)
            {
                Collider hitCollider = _targetsBuffer[i];

                EntityView targetView = hitCollider.GetComponentInParent<EntityView>();

                if (targetView == null)
                    continue;

                Entity targetEntity = targetView.Entity;

                if (targetEntity.IsNullOrDisposed()
                    || targetEntity == sourceBarrel
                    || processedEntities.Contains(targetEntity))
                    continue;

                if (!CanTakeDamage(targetEntity))
                    continue;
                
                processedEntities.Add(targetEntity);

                _statsService.CreateEffectModifier(
                    target: targetEntity,
                    targetStat: StatId.HEALTH,
                    operation: StatOperation.ADD,
                    modifierValue: damage,
                    producer: sourceBarrel,
                    position: hitCollider.transform.position
                );
            }
        }

        private bool CanTakeDamage(Entity entity)
        {
            if (entity.Has<CrowdMemberTag>() && !entity.Has<ZeroHealthMarker>())
                return true;

            if (entity.Has<InteractiveObjectTag>()
                && !entity.Has<ZeroHealthMarker>()
                && !entity.Has<ImmortalMarker>())
                return true;

            return false;
        }

        public void Dispose()
        {
        }
    }
}