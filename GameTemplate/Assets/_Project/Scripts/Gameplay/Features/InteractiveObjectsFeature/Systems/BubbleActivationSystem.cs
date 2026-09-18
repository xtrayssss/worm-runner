using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.EnhancementFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BubbleActivationSystem : ISystem
    {
        public World World { get; set; }

        private Filter _activatedBubbles;
        private Filter _crowds;
        private readonly EnhancementService _enhancementService;

        public BubbleActivationSystem(
            EnhancementService enhancementService)
        {
            _enhancementService = enhancementService;
        }

        public void OnAwake()
        {
            _activatedBubbles = World.Filter
                .With<BubbleTag>()
                .With<BubbleState>()
                .With<BubbleActivatedMarker>()
                .With<EntityViewLink>()
                .Without<BubbleDestroyMarker>()
                .Build();

            _crowds = World.Filter
                .With<CrowdTag>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_crowds.IsEmpty())
                return;

            Entity crowd = _crowds.First();

            foreach (Entity bubble in _activatedBubbles)
            {
                ref readonly EntityViewLink viewLink = ref bubble.GetComponent<EntityViewLink>();
                ref readonly BubbleState bubbleState = ref bubble.GetComponent<BubbleState>();

                _enhancementService.ApplyEffect(
                    source: bubble,
                    crowd,
                    effectValue: bubbleState.EffectValue * bubbleState.Step,
                    vfxPosition: viewLink.View.transform.position,
                    effectType: bubbleState.EffectType
                );

                bubble.AddComponent<BubbleDestroyMarker>();
            }
        }

        public void Dispose()
        {
        }
    }
}