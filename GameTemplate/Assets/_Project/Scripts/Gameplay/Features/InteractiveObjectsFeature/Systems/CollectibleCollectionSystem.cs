using System.Linq;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.CrowdFeature;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CollectibleCollectionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _collectibles;

        private readonly CollectibleService _collectibleService;
        private readonly UIRoot _uiRoot;

        public CollectibleCollectionSystem(CollectibleService collectibleService, UIRoot uiRoot)
        {
            _collectibleService = collectibleService;
            _uiRoot = uiRoot;
        }

        public void OnAwake()
        {
            _collectibles = World.Filter
                .With<CollectibleTag>()
                .With<CollectibleTypeLink>()
                .With<ActiveTrigger>()
                .Without<CollectibleDestroyMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity collectible in _collectibles)
            {
                ref readonly ActiveTrigger activeTrigger = ref collectible.GetComponent<ActiveTrigger>();

                if (activeTrigger.Triggers.Count == 0)
                    continue;

                if (CheckCollectionConditions(activeTrigger)) 
                    CollectItem(collectible);
            }
        }

        private bool CheckCollectionConditions(in ActiveTrigger trigger)
        {
            return trigger.Triggers.Any(static info =>
                !info.Other.Entity.IsNullOrDisposed() &&
                info.Other.Entity.Has<MilitaryCrowdMemberTag>());
        }

        private void CollectItem(Entity collectible)
        {
            ref readonly CollectibleTypeLink collectibleType = ref collectible.GetComponent<CollectibleTypeLink>();
            _collectibleService.AddCollectedCollectible(collectibleType.Value);

            collectible.AddComponent<CollectibleDestroyMarker>();

            _uiRoot.GameWindow.ObjectiveLabel.UpdateObjectiveDisplay();
        }

        public void Dispose()
        {
        }
    }
}