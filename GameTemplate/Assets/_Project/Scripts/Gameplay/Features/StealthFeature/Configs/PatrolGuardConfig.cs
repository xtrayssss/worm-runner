using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Configs
{
    [Serializable]
    public record PatrolGuardConfig : BaseInteractiveObjectConfig<PatrolGuardConfig>
    {
        [SerializeField]
        private StealthObjectConfig _stealthObjectConfig;
        
        [SerializeField]
        private float _rotationSpeed = 20f;

        [SerializeField]
        private float _maxRotationAngle = 45f;

        public float RotationSpeed => BaseConfig._rotationSpeed;
        public float MaxRotationAngle => BaseConfig._maxRotationAngle;
   
        public StealthObjectConfig StealthObjectConfig => BaseConfig._stealthObjectConfig;
    }
}