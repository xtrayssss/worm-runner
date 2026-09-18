using System;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs
{
    [Serializable]
    public record CaptureZoneConfig
    {
        [SerializeField, Min(1)]
        private int _requiredMembers = 3;

        public int RequiredMembers
        {
            get => _requiredMembers;
            set => _requiredMembers = value;
        }
    }
}