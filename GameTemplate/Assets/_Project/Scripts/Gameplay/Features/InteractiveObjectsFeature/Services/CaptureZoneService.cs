using System;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Components;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Services
{
    [Serializable]
    public sealed class CaptureZoneService : IService
    {
        private Filter _allCaptureZones;
        private readonly World _world;

        [ShowInInspector]
        private int _totalRequiredMembers;

        [ShowInInspector]
        private int _totalCapturedMembers;

        [ShowInInspector]
        public int RequiredMembersForCompletion => Mathf.CeilToInt(_totalRequiredMembers * _requiredPercent);
        
        public int TotalCapturedMembers => _totalCapturedMembers;

#if UNITY_EDITOR
        [ShowInInspector]
        public string DebugInfo => GetDebugInfo();
#endif

        [ShowInInspector]
        public bool HasRequiredCompletion { get; private set; }

        private float _requiredPercent;

        public CaptureZoneService()
        {
            _world = World.Default;

            _allCaptureZones = _world!.Filter
                .With<CaptureZoneTag>()
                .With<CaptureZoneState>()
                .Build();
        }

        public void Setup(float requiredPercent)
        {
            _requiredPercent = requiredPercent;
            _world.Commit();

            CalculateTotalRequiredMembers();
            UpdateCompletion();
        }

        private void CalculateTotalRequiredMembers()
        {
            _totalRequiredMembers = 0;

            foreach (Entity zone in _allCaptureZones)
            {
                ref readonly CaptureZoneState state = ref zone.GetComponent<CaptureZoneState>();
                _totalRequiredMembers += state.RequiredMembers;
            }
        }

        public void UpdateCompletion()
        {
            _totalCapturedMembers = 0;

            foreach (Entity zone in _allCaptureZones)
            {
                ref readonly CaptureZoneState state = ref zone.GetComponent<CaptureZoneState>();
                _totalCapturedMembers += state.CapturedMembers.Count;
            }

            HasRequiredCompletion = _totalCapturedMembers >= RequiredMembersForCompletion;
        }

        public void Reset()
        {
            _totalRequiredMembers = 0;
            _totalCapturedMembers = 0;
            HasRequiredCompletion = false;
        }

#if UNITY_EDITOR
        public string GetDebugInfo()
        {
            return $"Zones: {_allCaptureZones.GetLengthSlow()}, " +
                   $"Members: {_totalCapturedMembers}/{_totalRequiredMembers},";
        }
#endif
    }
}