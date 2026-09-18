using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
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
    public sealed class InteractiveObjectCrowdCollisionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _interactiveObjects;
        private readonly StatsService _statsService;

        public InteractiveObjectCrowdCollisionSystem(StatsService statsService)
        {
            _statsService = statsService;
        }

        public void OnAwake()
        {
            _interactiveObjects = World.Filter
                .With<InteractiveObjectTag>()
                .With<ActiveTrigger>()
                .With<ContactDamage>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity obj in _interactiveObjects)
            {
                ref ActiveTrigger activeTrigger = ref obj.GetComponent<ActiveTrigger>();

                if (activeTrigger.Triggers.Count == 0)
                    continue;

                foreach (ActiveTrigger.TriggerInfo triggerInfo in activeTrigger.Triggers)
                {
                    Entity crowdMember = triggerInfo.Other.Entity;

                    if (crowdMember.IsNullOrDisposed() ||
                        !crowdMember.Has<CrowdMemberTag>() ||
                        crowdMember.Has<ZeroHealthMarker>())
                        continue;

                    ApplyDamageToCrowdMember(obj, crowdMember, triggerInfo.Point);
                    ApplyDamageToInteractiveObject(obj, crowdMember, triggerInfo.Point);
                }

                activeTrigger.IsProcessed = true;
            }
        }

        private void ApplyDamageToCrowdMember(Entity obj, Entity crowdMember, Vector3 triggerPoint)
        {
            ref readonly ContactDamage contactDamage = ref obj.GetComponent<ContactDamage>();

            _statsService.CreateStatModifier(
                target: crowdMember,
                modifierSetup: new HealthModifierSetup
                {
                    TargetStatId = StatId.HEALTH,
                    Operation = contactDamage.Operation,
                    Priority = (int)contactDamage.Operation,
                    EffectType = StatEffectType.INSTANT,
                    ModifierValue = contactDamage.Damage
                },
                statSource: obj,
                point: triggerPoint,
                producer: obj);
        }

        private void ApplyDamageToInteractiveObject(Entity obj, Entity crowdMember, Vector3 triggerPoint)
        {
            if (obj.Has<ImmortalMarker>() ||
                obj.Has<ZeroHealthMarker>())
                return;

            if (!crowdMember.Has<Damage>())
                return;

            ref readonly Damage memberDamage = ref crowdMember.GetComponent<Damage>();

            _statsService.CreateStatModifier(
                target: obj,
                modifierSetup: new HealthModifierSetup
                {
                    TargetStatId = StatId.HEALTH,
                    Operation = StatOperation.ADD,
                    Priority = (int)StatOperation.ADD,
                    EffectType = StatEffectType.INSTANT,
                    ModifierValue = memberDamage.CurrentDamage
                },
                statSource: crowdMember,
                point: triggerPoint,
                producer: crowdMember);
        }

        public void Dispose()
        {
        }
    }
}