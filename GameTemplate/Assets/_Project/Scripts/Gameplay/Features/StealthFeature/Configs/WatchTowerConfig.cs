using System;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Configs
{
    [Serializable]
    public record WatchTowerConfig : BaseInteractiveObjectConfig<WatchTowerConfig>
    {
        [SerializeField]
        private StealthObjectConfig _stealthObjectConfig;

        [FormerlySerializedAs("_sweepSpeed")]
        [SerializeField]
        private float _sweepDuration = 15f;
    
        public float SweepDuration => BaseConfig._sweepDuration;
        public StealthObjectConfig StealthObjectConfig => BaseConfig._stealthObjectConfig;
    }
}