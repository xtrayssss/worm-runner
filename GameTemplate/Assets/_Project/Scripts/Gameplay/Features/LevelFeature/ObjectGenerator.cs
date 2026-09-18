using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using _Project.Scripts.Gameplay.Features.LevelFeature.Debug;
using JetBrains.Annotations;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature
{
    public static class ObjectGenerator
    {
#if UNITY_EDITOR
        public static LevelObjectData GenerateObjectFromPreset(ObjectGenerationPreset preset,
            Vector3 position,
            int? roadIndex = null)
        {
            LevelObjectData levelObjectData = preset.ObjectType switch
            {
                InteractiveObjectId.ENEMY_GROUP => GenerateObject<EnemyGroup>(
                    preset.ObjectType,
                    position,
                    roadIndex),

                _ => GenerateObject<LevelObjectData>(preset.ObjectType, position, roadIndex)
            };

            preset.ApplyToConfig(levelObjectData.CustomData);

            return levelObjectData;
        }
#endif

        public static TLevelObjectData GenerateObject<TLevelObjectData>(
            InteractiveObjectId id,
            Vector3 position,
            int? roadIndex = null,
            [CanBeNull] InteractiveObjectData customData = null)
            where TLevelObjectData : LevelObjectData, new()
        {
            TLevelObjectData levelObjectData = new TLevelObjectData
            {
                ObjectType = id,
                Position = position,
#if UNITY_EDITOR
                RoadIndex = roadIndex ?? -1,
#endif
                CustomData = customData ?? new InteractiveObjectData
                {
                    Id = id
                }
            };

            if (customData != null)
                customData.Id = id;

            return levelObjectData;
        }
    }
}
