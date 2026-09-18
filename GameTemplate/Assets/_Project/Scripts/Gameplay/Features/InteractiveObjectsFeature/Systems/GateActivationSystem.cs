using System.Linq;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GateActivationSystem : ISystem
    {
        public World World { get; set; }

        private Filter _gates;
        private Filter _crowds;

        private readonly EnhancementService _enhancementService;

        public GateActivationSystem(EnhancementService enhancementService) =>
            _enhancementService = enhancementService;

        public void OnAwake()
        {
            _gates = World.Filter
                .With<GateTag>()
                .With<GateState>()
                .With<ActiveTrigger>()
                .Without<GateDestroyMarker>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity gate in _gates)
            {
                ref readonly ActiveTrigger trigger = ref gate.GetComponent<ActiveTrigger>();

                bool shouldActivate = CheckActivationConditions(trigger);

                if (shouldActivate)
                    ActivateGate(gate);
            }
        }

        private void ActivateGate(Entity gate)
        {
            Entity crowd = _crowds.First();

            ref readonly EntityViewLink crowdViewLink = ref crowd.GetComponent<EntityViewLink>();
            Vector3 crowdPosition = crowdViewLink.View.transform.position;

            ref readonly GateState gateState = ref gate.GetComponent<GateState>();

            float effectValue = gateState.EffectValue;

            if (gateState.EffectType == EffectType.STAT_MODIFICATION)
                effectValue *= gateState.Step;

            _enhancementService.ApplyEffect(
                source: gate,
                crowd,
                effectValue,
                vfxPosition: crowdPosition,
                effectType: gateState.EffectType);

            gate.AddComponent<GateDestroyMarker>();
        }

        private static bool CheckActivationConditions(in ActiveTrigger trigger)
        {
            return trigger.Triggers
                .Any(static triggerInfo => !triggerInfo.Other.Entity.IsNullOrDisposed() &&
                                           triggerInfo.Other.Entity.Has<MilitaryCrowdMemberTag>());
        }

        public void Dispose()
        {
        }
    }
}