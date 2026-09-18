using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.CollisionFeature
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CollisionCleanupSystem : ICleanupSystem
    {
        public World World { get; set; }

        private Filter _entitiesWithCollisions;
        private Filter _entitiesWithTriggers;
        private Filter _entitiesWithUnifiedCollisions;

        public void OnAwake()
        {
            _entitiesWithCollisions = World.Filter
                .With<ActiveCollision>()
                .Build();

            _entitiesWithTriggers = World.Filter
                .With<ActiveTrigger>()
                .Build();

            _entitiesWithUnifiedCollisions = World.Filter
                .With<UnifiedCollision>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity entity in _entitiesWithCollisions)
            {
                ref ActiveCollision activeCollision = ref entity.GetComponent<ActiveCollision>();

                if (!activeCollision.IsProcessed)
                    continue;

                activeCollision.Collisions.Clear();
                entity.RemoveComponent<ActiveCollision>();
            }

            foreach (Entity entity in _entitiesWithTriggers)
            {
                ref ActiveTrigger activeTrigger = ref entity.GetComponent<ActiveTrigger>();

                if (!activeTrigger.IsProcessed)
                    continue;

                activeTrigger.Triggers.Clear();
                entity.RemoveComponent<ActiveTrigger>();
            }

            foreach (Entity entity in _entitiesWithUnifiedCollisions)
            {
                ref UnifiedCollision unifiedCollision = ref entity.GetComponent<UnifiedCollision>();

                if (!unifiedCollision.IsProcessed)
                    continue;

                unifiedCollision.Contacts.Clear();
                entity.RemoveComponent<UnifiedCollision>();
            }
        }

        public void Dispose()
        {
        }
    }
}