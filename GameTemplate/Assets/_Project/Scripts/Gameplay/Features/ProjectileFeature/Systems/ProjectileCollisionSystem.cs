using System;
using _Project.Scripts.Gameplay.Features.CollisionFeature.Components;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using _Project.Scripts.Gameplay.Features.StatsFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace _Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileCollisionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _projectiles;

        public void OnAwake() =>
            _projectiles = World.Filter
                .With<ProjectileTag>()
                .With<UnifiedCollision>()
                .With<CollisionLimit>()
                .Build();

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity projectile in _projectiles)
            {
                ref readonly UnifiedCollision unifiedCollision = ref projectile.GetComponent<UnifiedCollision>();
                ref CollisionLimit collisionLimit = ref projectile.GetComponent<CollisionLimit>();

                int collisionsBeforeThisFrame = collisionLimit.CurrentCollisions;
                int totalCollisionsAllowed = collisionLimit.Limit;
                int remainingAllowedCollisions = Math.Max(0, totalCollisionsAllowed - collisionsBeforeThisFrame);
                int availableCollisionsThisFrame = unifiedCollision.ContactCount;
                int maxCollisionsToProcess = Math.Min(remainingAllowedCollisions, availableCollisionsThisFrame);

                int processedCollisions = 0;

                for (int i = 0;
                     i < unifiedCollision.Contacts.Count && processedCollisions < maxCollisionsToProcess;
                     i++)
                {
                    UnifiedCollision.UnifiedCollisionInfo contact = unifiedCollision.Contacts[i];

                    if (contact.Other.Entity.IsNullOrDisposed() || 
                        contact.Other.Entity.Has<ZeroHealthMarker>())
                        continue;

                    World
                        .GetRequest<ProjectileImpactRequest>()
                        .Publish(new ProjectileImpactRequest
                        {
                            Damageable = contact.Other.Entity,
                            Projectile = projectile,
                            Point = contact.Point
                        });

                    processedCollisions++;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}