using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [Serializable]
    public abstract class ObjectGenerationPreset
    {
        [SerializeField] 
        protected float _weight = 1f;

        public float Weight => _weight;

        public abstract InteractiveObjectId ObjectType { get; }
        public abstract void ApplyToConfig(InteractiveObjectData config);
    }
}