using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public sealed class MortarStrikeData
    {
        [SerializeField]
        private float _triggerTime;
        
        public float TriggerTime => _triggerTime;
    }
}