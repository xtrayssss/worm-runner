using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.StealthFeature.Configs
{
    [Serializable]
    public record StealthObjectConfig
    {
        [SerializeField] 
        private float _visionRange = 10f;
        
        [SerializeField] 
        [Range(0f, 360f)]
        private float _visionAngle = 90f;
        
        public float VisionRange => _visionRange;
        public float VisionAngle => _visionAngle;
    }
}