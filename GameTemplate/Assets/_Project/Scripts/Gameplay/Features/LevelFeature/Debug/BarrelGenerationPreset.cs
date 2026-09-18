#if UNITY_EDITOR
using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public class BarrelGenerationPreset : ObjectGenerationPreset
    {
        [TabGroup("Barrel Settings")]
        [SerializeField] private Vector2 _maxHealthRange = new Vector2(15f, 30f);

        public override InteractiveObjectId ObjectType => InteractiveObjectId.BARREL;

        public override void ApplyToConfig(InteractiveObjectData config)
        {
            config.MaxHealth = Random.Range(_maxHealthRange.x, _maxHealthRange.y);
        }
    }
}
#endif