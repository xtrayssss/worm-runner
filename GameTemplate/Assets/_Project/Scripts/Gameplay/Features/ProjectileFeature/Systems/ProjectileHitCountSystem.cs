using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileHitCountSystem : ISystem
    {
        public World World { get; set; }

        private Event<DamagedEvent> _damagedEvents;

        public void OnAwake()
        {
            _damagedEvents = World.GetEvent<DamagedEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (DamagedEvent damagedEvent in _damagedEvents.publishedChanges)
            {
                Entity projectile = damagedEvent.Modifier.Producer;

                if (projectile.IsNullOrDisposed() ||
                    !projectile.Has<CollisionLimit>() ||
                    !projectile.Has<ProjectileTag>() ||
                    !projectile.Has<UnifiedCollision>())
                {
                    continue;
                }

                ref CollisionLimit collisionLimit = ref projectile.GetComponent<CollisionLimit>();
                collisionLimit.CurrentCollisions++;

                ref UnifiedCollision unifiedCollision = ref projectile.GetComponent<UnifiedCollision>();
                unifiedCollision.IsProcessed = true;
            }
        }

        public void Dispose()
        {
        }
    }
}