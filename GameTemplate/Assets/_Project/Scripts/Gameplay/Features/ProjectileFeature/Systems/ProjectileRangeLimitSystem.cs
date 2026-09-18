using _Project.Scripts.Gameplay.Features.CommonFeature.Components;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.ProjectileFeature.Components;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.Scripts.Gameplay.Features.ProjectileFeature.Systems
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileRangeLimitSystem : ISystem
    {
        private Filter _projectiles;
        public World World { get; set; }

        public void OnAwake()
        {
            _projectiles = World.Filter
                .With<ProjectileTag>()
                .With<EntityViewLink>()
                .With<ProjectileDistanceTracker>()
                .Without<ProjectileDeathMarker>()
                .Build();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (Entity projectile in _projectiles)
            {
                ref EntityViewLink viewLink = ref projectile.GetComponent<EntityViewLink>();
                ref ProjectileDistanceTracker tracker = ref projectile.GetComponent<ProjectileDistanceTracker>();

                float traveledDistance = Vector3.Distance(tracker.StartPoint, viewLink.View.transform.position);

                if (traveledDistance > tracker.MaxDistance)
                {
                    projectile.AddComponent<ProjectileDeathMarker>();
#if DEBUG
                    // ref readonly EntityName projectileName = ref projectile.GetComponent<EntityName>();
                    // Debug.Log($"Entity {projectileName.Value} exceeded distance limit of {tracker.MaxDistance}. Removing.");
#endif
                }
            }
        }

        public void Dispose()
        {
        }
    }
}