using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.DamageFeature.Components;
using _Project.Scripts.Gameplay.Features.MovementFeature.Components;
using Scellecs.Morpeh;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace _Project.Scripts.Gameplay.Features.DamageFeature.Systems
{
    public sealed class TargetingService : IService
    {
        public static bool IsInVisionCone(
            Vector3 origin,
            Vector3 forward,
            Vector3 target,
            float range,
            float angle)
        {
            float distanceSqr = (target - origin).sqrMagnitude;
            if (distanceSqr > range * range)
                return false;

            Vector3 direction = target - origin;
            direction.y = 0;
            Vector3 horizontalForward = forward;
            horizontalForward.y = 0;

            if (direction.sqrMagnitude < 0.001f || horizontalForward.sqrMagnitude < 0.001f)
                return distanceSqr < 0.001f;

            direction.Normalize();
            horizontalForward.Normalize();

            float dot = Vector3.Dot(direction, horizontalForward);
            float requiredCos = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            return dot >= requiredCos;
        }

        public Entity SelectTarget(Filter filter, TargetType targetType, Entity source, float range = float.MaxValue)
        {
            List<Entity> entitiesInRange = GetEntitiesInRange(filter, source.GetEntityPosition(), range);

            if (entitiesInRange.Count == 0)
                return default;

            return targetType switch
            {
                TargetType.NEAREST => FindNearestEntity(entitiesInRange, source),
                TargetType.FARTHEST => FindFarthestEntity(entitiesInRange, source),
                TargetType.RANDOM => entitiesInRange[UnityEngine.Random.Range(0, entitiesInRange.Count)],
                TargetType.WITH_LOWEST_HEALTH => FindEntityWithLowestHealth(entitiesInRange),
                TargetType.WITH_HIGHEST_HEALTH => FindEntityWithHighestHealth(entitiesInRange),
                _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
            };
        }

        public bool HasTargetsInRange(Filter filter, Vector3 sourcePosition, float range)
        {
            return GetEntitiesInRange(filter, sourcePosition, range).Count > 0;
        }

        public (Entity entity, float distance) FindNearestEntityInRange(Filter filter, Entity source, float range,
            bool excludeSource = true)
        {
            Vector3 sourcePosition = source.GetEntityPosition();

            float closestDistance = float.MaxValue;
            Entity closestEntity = default;

            foreach (Entity entity in filter)
            {
                if (excludeSource && entity.Id == source.Id)
                    continue;

                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance <= range && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEntity = entity;
                }
            }

            return closestEntity.IsNullOrDisposed()
                ? (default, 0f)
                : (closestEntity, closestDistance);
        }

        public (Entity entity, float distance) FindFarthestEntityInRange(Filter filter, Entity source, float range,
            bool excludeSource = true)
        {
            Vector3 sourcePosition = source.GetEntityPosition();

            float farthestDistance = 0f;
            Entity farthestEntity = default;

            foreach (Entity entity in filter)
            {
                if (excludeSource && entity.Id == source.Id)
                    continue;

                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance <= range && distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestEntity = entity;
                }
            }

            return farthestEntity.IsNullOrDisposed()
                ? (default, 0f)
                : (farthestEntity, farthestDistance);
        }

        public List<Entity> GetEntitiesInRange(Filter filter, Vector3 sourcePosition, float range)
        {
            List<Entity> entitiesInRange = new List<Entity>(capacity: 4);

            foreach (Entity entity in filter)
            {
                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance <= range)
                {
                    entitiesInRange.Add(entity);
                }
            }

            return entitiesInRange;
        }

        public void GetEntitiesInRangeNonAlloc(Filter filter, Vector3 sourcePosition, float range, List<Entity> results)
        {
            results.Clear();

            foreach (Entity entity in filter)
            {
                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance <= range)
                {
                    results.Add(entity);
                }
            }
        }

        public List<Entity> GetEntitiesInRange(List<Entity> entities, Vector3 sourcePosition, float range)
        {
            List<Entity> entitiesInRange = new List<Entity>();

            foreach (Entity entity in entities)
            {
                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance <= range)
                {
                    entitiesInRange.Add(entity);
                }
            }

            return entitiesInRange;
        }

        public List<Entity> GetEntitiesInRangeSorted(
            Filter filter,
            Vector3 sourcePosition,
            float range,
            bool ascending = true)
        {
            List<Entity> entitiesInRange = GetEntitiesInRange(filter, sourcePosition, range);

            entitiesInRange.Sort((a, b) =>
            {
                Vector3 posA = a.GetEntityPosition();
                Vector3 posB = b.GetEntityPosition();
                float distA = Vector3.Distance(sourcePosition, posA);
                float distB = Vector3.Distance(sourcePosition, posB);
                return ascending ? distA.CompareTo(distB) : distB.CompareTo(distA);
            });

            return entitiesInRange;
        }

        public List<Entity> GetEntitiesInRangeSorted(
            List<Entity> entities,
            Entity source,
            float range,
            bool ascending = true)
        {
            Vector3 sourcePosition = source.GetEntityPosition();

            List<Entity> entitiesInRange = GetEntitiesInRange(entities, sourcePosition, range);

            entitiesInRange.Sort((a, b) =>
            {
                Vector3 posA = a.GetEntityPosition();
                Vector3 posB = b.GetEntityPosition();
                float distA = Vector3.Distance(sourcePosition, posA);
                float distB = Vector3.Distance(sourcePosition, posB);
                return ascending ? distA.CompareTo(distB) : distB.CompareTo(distA);
            });

            return entitiesInRange;
        }

        public Entity GetRandomEntityInRange(Filter filter, Entity source, float range)
        {
            List<Entity> entitiesInRange = GetEntitiesInRange(filter, source.GetEntityPosition(), range);

            return entitiesInRange.Count == 0
                ? default
                : entitiesInRange[UnityEngine.Random.Range(0, entitiesInRange.Count)];
        }

        public int CountEntitiesInRange(Filter filter, Entity source, float range)
        {
            return GetEntitiesInRange(filter, source.GetEntityPosition(), range).Count;
        }

        private static Entity FindNearestEntity(List<Entity> entities, Entity source)
        {
            if (entities.Count == 0)
                return default;

            Vector3 sourcePosition = source.GetEntityPosition();

            float closestDistance = float.MaxValue;
            Entity closestEntity = default;

            foreach (Entity entity in entities)
            {
                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEntity = entity;
                }
            }

            return closestEntity;
        }

        private static Entity FindFarthestEntity(List<Entity> entities, Entity source)
        {
            if (entities.Count == 0)
                return default;

            Vector3 sourcePosition = source.GetEntityPosition();

            float farthestDistance = 0f;
            Entity farthestEntity = default;

            foreach (Entity entity in entities)
            {
                Vector3 entityPosition = entity.GetEntityPosition();
                float distance = Vector3.Distance(sourcePosition, entityPosition);

                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestEntity = entity;
                }
            }

            return farthestEntity;
        }

        private static Entity FindEntityWithLowestHealth(List<Entity> entities)
        {
            if (entities.Count == 0)
                return default;

            Entity entityWithLowestHealth = default;
            float lowestHealth = float.MaxValue;

            foreach (Entity entity in entities)
            {
                // Предполагаем, что есть компонент Health
                if (entity.Has<Health>())
                {
                    ref Health health = ref entity.GetComponent<Health>();
                    if (health.CurrentHealth < lowestHealth)
                    {
                        lowestHealth = health.CurrentHealth;
                        entityWithLowestHealth = entity;
                    }
                }
            }

            return entityWithLowestHealth;
        }

        private static Entity FindEntityWithHighestHealth(List<Entity> entities)
        {
            if (entities.Count == 0)
                return default;

            Entity entityWithHighestHealth = default;
            float highestHealth = 0f;

            foreach (Entity entity in entities)
            {
                // Предполагаем, что есть компонент Health
                if (entity.Has<Health>())
                {
                    ref Health health = ref entity.GetComponent<Health>();
                    if (health.CurrentHealth > highestHealth)
                    {
                        highestHealth = health.CurrentHealth;
                        entityWithHighestHealth = entity;
                    }
                }
            }

            return entityWithHighestHealth;
        }
    }
}