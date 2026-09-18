using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GateVisualSystem : ISystem, IService
    {
        public World World { get; set; }

        private Filter _gatesWithDisplay;

        private readonly EnhancementService _enhancementService;

        public GateVisualSystem(EnhancementService enhancementService) =>
            _enhancementService = enhancementService;

        public void OnAwake()
        {
            _gatesWithDisplay = World.Filter
                .With<GateTag>()
                .With<GateState>()
                .With<EntityViewLink>()
                .Without<GateDestroyMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity gate in _gatesWithDisplay)
            {
                ref readonly EntityViewLink viewLink = ref gate.GetComponent<EntityViewLink>();

                GateView view = (GateView)viewLink.View;

                ref readonly GateState gateState = ref gate.GetComponent<GateState>();

                view.UpdateEffectValueLabel(gateState.EffectValue);

                UpdateGateSprite(gate, view);
            }
        }

        public void UpdateGateSprite(Entity gate, GateView view)
        {
            ref readonly GateState gateState = ref gate.GetComponent<GateState>();

            bool isBuff = gateState.EffectValue > 0f;

            view.SetSprite(isBuff);
        }

        public void Dispose()
        {
        }
    }
}